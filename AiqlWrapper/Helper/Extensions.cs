using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AiqlWrapper.Helper
{
    internal static class Extensions
    {
        public static int IndexOf<T>(this IEnumerable<T> src, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in src)
            {
                if (predicate(item))
                    return i;
                i++;
            }
            throw new InvalidOperationException("Sequence does not contain any matching item");
        }

        public static string GetMemberName(this MemberExpression expr)
        {
            FieldNameAttribute nm;
            if ((nm = expr.Member.GetCustomAttribute<FieldNameAttribute>()) != null)
                return nm.Name;
            else
                return expr.Member.Name;
        }

        public static IEnumerable<TExpression> GetExpressions<TExpression>(this IEnumerable<Expression> source, ExpressionType? type = null) where TExpression : Expression
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            foreach (var expr in source)
            {
                var texpr = expr as TExpression;
                if (texpr != null && (type == null || texpr.NodeType == type))
                    yield return texpr;
            }
        }
    }
}
