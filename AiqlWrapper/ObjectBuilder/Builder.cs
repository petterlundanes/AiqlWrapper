using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Helper;

namespace AiqlWrapper.ObjectBuilder
{
    internal interface IBuilder
    {
        IEnumerable<T> Build<T>(ResultTable rt);
    }
    internal class Builder : IBuilder
    {
        public static Builder CreateFromCtor(ConstructorInfo ctor, int[] ctorIds)
        {
            var builder = new Builder();
            builder.AddConstructor(ctor, ctorIds);
            return builder;
        }

        public static Builder CreateFromMemberInit(ConstructorInfo ctor, int[] ctorIds, IEnumerable<MemberInfo> bindings, int[] minitIds)
        {
            var builder = new Builder();
            builder.AddConstructor(ctor, ctorIds);
            builder.AddMemberInit(bindings, minitIds);
            return builder;
        }
        public static Builder CreateFromMemberAccess(Type memberType)
        {
            var builder = new Builder();
            builder.AsMember(memberType);
            return builder;
        }

        protected Builder()
        {
            Projector = new TableProjector();
            ProjectorExpression = Expression.Constant(Projector);
            ProjectorValueGetter = typeof(TableProjector).GetMethod(nameof(TableProjector.GetValue),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
        protected TableProjector Projector { get; }
        protected ConstantExpression ProjectorExpression { get; }
        protected MethodInfo ProjectorValueGetter { get; }

        public IEnumerable<T> Build<T>(ResultTable rt)
        {
            Projector.Results = rt;
            return Projector.Project(Expression.Lambda<Func<T>>(_buildExpression).Compile());
        }

        private Expression _buildExpression;

        private void AddConstructor(ConstructorInfo ctor, int[] ids)
        {
            if (_buildExpression != null) throw new Exception("Multiple calls to AddConstructor is not supported.");

            var exprs = ctor.GetParameters()
                .Select((pi, index) => (Expression) Expression.Call(ProjectorExpression,
                    ProjectorValueGetter.MakeGenericMethod(pi.ParameterType), Expression.Constant(ids[index])))
                .ToArray();
            _buildExpression = Expression.New(ctor, exprs);
        }

        private void AddMemberInit(IEnumerable<MemberInfo> members, int[] ids)
        {
            if (!(_buildExpression is NewExpression)) throw new Exception("Must call AddConstructor before this");

            var binds = members.Select((mem, idx) =>
            {
                var type = ((PropertyInfo) mem).PropertyType;
                var expr = (Expression) Expression.Call(ProjectorExpression,
                    ProjectorValueGetter.MakeGenericMethod(type), Expression.Constant(ids[idx]));
                return (MemberBinding)Expression.Bind(mem, expr);
            }).ToArray();

            _buildExpression = Expression.MemberInit((NewExpression)_buildExpression, binds);
        }
        private void AsMember(Type memberType)
        {
            Expression expr = Expression.Call(ProjectorExpression, ProjectorValueGetter.MakeGenericMethod(memberType),
                Expression.Constant(0));
            _buildExpression = expr;
        }
    }
    internal class TableProjector
    {
        public ResultTable Results { get; set; }

        private int _index;
        public IEnumerable<T> Project<T>(Func<T> builder)
        {
            for (_index = 0; _index < Results.Rows.Length; _index++)
            {
                yield return builder();
            }
        }

        internal TElement GetValue<TElement>(int i)
        {
            try
            {
                var obj = Results.Rows[_index][i];
                if (typeof(TElement) == typeof(double))
                {
                    if (obj is double)
                        return (TElement) obj;
                    if (obj is int integer)
                        return (TElement)(object)Convert.ToDouble(integer);
                    if (obj is long integer2)
                        return (TElement)(object)Convert.ToDouble(integer2);
                }
                else if (typeof(TElement) == typeof(int))
                {
                    if (obj is long l)
                        return (TElement)(object)(int)l;
                }
                return (TElement)obj;
            }
            catch (InvalidCastException)
            {
                var col = Results.Columns[i];
                var telement = typeof(TElement);
                var typ = Results.Rows[_index][i].GetType();
                throw;
            }
        }
    }
}
