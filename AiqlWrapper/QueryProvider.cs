using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AiqlWrapper.Helper;
using AiqlWrapper.ObjectBuilder;
using AiqlWrapper.Tables;
using AiqlWrapper.Visitors;
using Newtonsoft.Json;

namespace AiqlWrapper
{
    internal class QueryProvider : IQueryProvider
    {
        private readonly ApplicationInsightsClient _client;

        internal QueryProvider(ApplicationInsightsClient client)
        {
            _client = client;
            _aggregationFinder = new IsAggregationVisitor();
        }
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Query<TElement>(this, expression);
        }
        
        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
        public TResult Execute<TResult>(Expression expression)
        {
            var res = ExecuteEnumerable<TResult>(expression).ToList();
            if (expression is MethodCallExpression mce)
            {
                if (mce.Method.DeclaringType == typeof(Queryable))
                {
                    bool orDefault;
                    bool first;
                    switch (mce.Method.Name)
                    {
                        case nameof(Queryable.Count):
                            return res[0];
                        case nameof(Queryable.LongCount):
                            return res[0];
                        case nameof(Queryable.FirstOrDefault):
                            first = true;
                            orDefault = true;
                            break;
                        case nameof(Queryable.First):
                            first = true;
                            orDefault = false;
                            break;
                        case nameof(Queryable.SingleOrDefault):
                            first = false;
                            orDefault = true;
                            break;
                        case nameof(Queryable.Single):
                            first = false;
                            orDefault = false;
                            break;
                        default:
                            throw new NotImplementedException(mce.Method.Name);
                    }
                    if (res.Count == 0)
                    {
                        if (orDefault)
                            return default(TResult);
                        throw new InvalidOperationException("The input sequence is empty.");
                    }
                    else if (res.Count == 1 || first)
                    {
                        return res[0];
                    }
                    else
                    {
                        throw new InvalidOperationException("The input sequence contains more than one element.");
                    }
                }
            }
            throw new NotImplementedException();
        }

        public IEnumerable<TResult> ExecuteEnumerable<TResult>(Expression expression)
        {
            PrintTree(expression, "Original tree");

            expression = LocalExpressionEvaluator.Evaluate(expression);
            PrintTree(expression, "Locals evaluated");

            if (_client.UseTopOptimization)
            {
                bool change;
                expression = TopOptimizer.Optimize(expression, out change);
                if (change)
                    PrintTree(expression, "TopOptimized");
            }
            Console.WriteLine("^ Final tree ^");
            var query = MakeQuery(expression);
            Console.WriteLine(query);

#if true
            var res = _client.RunAnalytics(query);
#else
            string res;
            if (typeof(TResult) == typeof(Request))
                res = DummyData.DummyJsonResultFull;
            else
                res = DummyData.DummyJsonResult;
#endif

            var tbls = JsonConvert.DeserializeObject<TablesWrapper>(res);
            if (_builder == null) _builder = DefaultBuilderFactory.Get<TResult>();
            var items = _builder.Build<TResult>(tbls.Tables[0]);
            return items;
        }

        private static void PrintTree(Expression expr, string description)
        {
            Console.WriteLine(description);
            var vistor = new PrinterVisitor();
            vistor.Visit(expr);
        }

        private string MakeQuery(Expression expression)
        {
            _builder = null;
            _negateNext = false;
            var sb = new StringBuilder();
            Visit(expression, sb);
            return sb.ToString();
        }

        private bool _negateNext;
        private void Visit(Expression expression, StringBuilder sb)
        {
            switch (expression)
            {
                case MethodCallExpression call:
                    HandleCall(sb, call);
                    break;
                case ConstantExpression constExpr:
                    if (constExpr.Value is IQueryable q)
                        sb.Append(q.ElementType.Name.ToLower() + "s");
                    else
                        sb.Append(GetConstAsAiql(constExpr.Value));
                    break;
                case UnaryExpression exp:
                    if (exp.NodeType == ExpressionType.Not)
                    {
                        TranslateBinaryMethodAttribute translate;
                        if (exp.Operand is MethodCallExpression mce && (mce.Method.DeclaringType == typeof(string) 
                            || ((translate = mce.Method.GetCustomAttribute<TranslateBinaryMethodAttribute>()) != null) && translate.Negatable))
                        {
                            _negateNext = true;
                            Visit(exp.Operand, sb);
                            if (_negateNext) throw new Exception("Negation not correctly handled");
                        }
                        else
                        {
                            sb.Append("not(");
                            Visit(exp.Operand, sb);
                            sb.Append(")");
                        }
                    }
                    else
                    {
                        Visit(exp.Operand, sb);
                    }
                    break;
                case LambdaExpression exp:
                    Visit(exp.Body, sb);
                    break;
                case BinaryExpression exp:
                    HandleBinary(sb, exp);
                    break;
                case MemberExpression exp:
                    if (exp.Member.MemberType == MemberTypes.Property)
                    {
                        if (exp.Expression.NodeType != ExpressionType.Parameter)
                        {
                            Visit(exp.Expression, sb);
                            sb.Append('.');
                        }
                        sb.Append(PadFieldName(exp.GetMemberName()));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case ConditionalExpression ce:
                    TranslateFunc("iff", sb, ce.Test, ce.IfTrue, ce.IfFalse);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static string PadFieldName(string name)
        {
            return $"['{name}']";
        }

        private void HandleBinary(StringBuilder sb, BinaryExpression exp)
        {
            Expression otherExpr = null;
            if (exp.Left is ConstantExpression ce && ce.Value == null)
                otherExpr = exp.Right;
            if (exp.Right is ConstantExpression ce2 && ce2.Value == null)
            {
                if (otherExpr != null) throw new Exception("null == null check not allowed");
                otherExpr = exp.Left;
            }
            if (otherExpr != null)
            {
                string func;
                if (exp.NodeType == ExpressionType.Equal)
                    func = "isnull";
                else if (exp.NodeType == ExpressionType.NotEqual)
                    func = "isnotnull";
                else throw new Exception("null comparison only allowed on equal/not-equal");
                sb.Append($"{func}(");
                Visit(otherExpr, sb);
                sb.Append(')');
                return;
            }
            sb.Append('(');
            Visit(exp.Left, sb);
            sb.Append($" {GetOperator(exp.NodeType)} ");
            Visit(exp.Right, sb);
            sb.Append(')');
        }

        private void HandleCall(StringBuilder sb, MethodCallExpression call)
        {
            TranslateBinaryMethodAttribute translate;
            if (call.Method.CustomAttributes.Any(c => c.AttributeType == typeof(TranslateFuncAttribute)))
            {
                TranslateFunc(call.Method.Name, sb, call.Arguments);
            }
            else if ((translate = call.Method.GetCustomAttributes<TranslateBinaryMethodAttribute>().FirstOrDefault()) != null)
            {
#if DEBUG
                if (_negateNext && !translate.Negatable)
                    throw new Exception("Bug. Has negateNext on non-negatable function");
#endif
                if (_negateNext)
                {
                    _negateNext = false;
                    WriteBinaryMethod(sb, call, "!" + translate.Func);
                }
                else
                    WriteBinaryMethod(sb, call, translate.Func);
            }
            else if (call.Method.DeclaringType == typeof(Queryable) ||
                     call.Method.DeclaringType == typeof(QueryExtensions))
            {
                if (call.Method.Name == nameof(Queryable.Where))
                {
                    Visit(call.Arguments[0], sb);
                    sb.Append("\n | where ");
                    Visit(call.Arguments[1], sb);
                }
                else if (call.Method.Name == nameof(Queryable.Count) || call.Method.Name == nameof(Queryable.LongCount))
                {
                    if (_builder == null)
                        _builder = Builder.CreateFromMemberAccess(call.Method.ReturnType);
                    Visit(call.Arguments[0], sb);
                    if (call.Arguments.Count > 1)
                    {
                        sb.Append("\n | where ");
                        Visit(call.Arguments[1], sb);
                    }
                    sb.Append($"\n | summarize {PadFieldName("count")}=count()");
                }
                else if (call.Method.Name == nameof(Queryable.Select))
                {
                    HandleSelect(sb, call);
                }
                else if (call.Method.Name.StartsWith(nameof(Queryable.OrderBy)) ||
                         call.Method.Name.StartsWith(nameof(QueryExtensions.Top)))
                {
                    var desc = call.Method.Name.EndsWith(Constants.DescendingPostfix);
                    var top = call.Method.Name.StartsWith(nameof(QueryExtensions.Top));
                    Visit(call.Arguments[0], sb);
                    // | sort by <last>
                    // | top n by <last>
                    var keySelectorIdx = 1;
                    if (top)
                    {
                        sb.Append($"\n | top {((ConstantExpression) call.Arguments[1]).Value} by ");
                        keySelectorIdx = 2;
                    }
                    else
                    {
                        sb.Append("\n | sort by ");
                    }
                    Visit(call.Arguments[keySelectorIdx], sb);
                    sb.Append(desc ? " desc" : " asc");
                }
                else if (call.Method.Name == nameof(Queryable.ThenBy) ||
                         call.Method.Name == nameof(Queryable.ThenByDescending))
                {
                    bool desc = call.Method.Name == nameof(Queryable.ThenByDescending);
                    Visit(call.Arguments[0], sb);
                    sb.Append(", ");
                    Visit(call.Arguments[1], sb);
                    sb.Append(desc ? " desc" : " asc");
                }
                else if (call.Method.Name == nameof(QueryExtensions.NullsFirst) ||
                         call.Method.Name == nameof(QueryExtensions.NullsLast))
                {
                    var last = call.Method.Name == nameof(QueryExtensions.NullsLast);
                    Visit(call.Arguments[0], sb);
                    sb.Append($" nulls {(last ? "last" : "first")}");
                }
                else if (call.Method.Name == nameof(Queryable.Take))
                {
                    Visit(call.Arguments[0], sb);
                    sb.Append("\n | take ");
                    Visit(call.Arguments[1], sb);
                }
                else if (call.Method.Name.StartsWith(nameof(Queryable.Single)) ||
                         call.Method.Name.StartsWith(nameof(Queryable.First)))
                {
                    var n = call.Method.Name.StartsWith(nameof(Queryable.First)) ? 1 : 2;
                    Visit(call.Arguments[0], sb);
                    if (call.Arguments.Count == 2)
                    {
                        sb.Append($"\n | where ");
                        Visit(call.Arguments[1], sb);
                    }
                    sb.Append($"\n | take {n}");
                }
                else
                {
                    throw new NotImplementedException($"Not implemented IQueryable extension: {call.Method.Name}");
                }
            }
            else if (call.Method.DeclaringType == typeof(string))
            {
                string keyWord;
                if (call.Method.Name == nameof(string.StartsWith))
                {
                    keyWord = "startswith";
                }
                else if (call.Method.Name == nameof(string.EndsWith))
                {
                    keyWord = "endswith";
                }
                else if (call.Method.Name == nameof(string.Contains))
                {
                    keyWord = "containscs";
                }
                else if (call.Method.Name == nameof(string.Equals))
                {
                    // Case insensitive if it has the third StringComparsion argument and that specifies a case insensitive comparer.
                    var cs = !(call.Arguments.Count == 3 && call.Arguments[2] is ConstantExpression ce &&
                        ce.Value is StringComparison strCmp && IsCaseInsensitive(strCmp));
                    keyWord = _negateNext ? "!" : "=";
                    keyWord += cs ? "=" : "~";
                    _negateNext = false;
                }
                else
                {
                    throw new NotImplementedException($"Not implemented string extension: {call.Method.Name}");
                }
                if (_negateNext)
                {
                    _negateNext = false;
                    keyWord = "!" + keyWord;
                }
                WriteBinaryMethod(sb, call, keyWord);
            }
            else if (call.Method.Name == "get_Item" && call.Object?.Type == typeof(ICustomDimensions))
            {
                sb.Append("customDimensions.");
                var arg = call.Arguments.Single();
                if (arg is ConstantExpression ce && ce.Value is string str)
                    sb.Append(PadFieldName(str));
                else
                    Visit(call.Arguments.Single(), sb);
            }
            else
                throw new NotImplementedException($"Not implemeneted method: {call.Method} in {call.Method.DeclaringType?.FullName}.");
        }

        private void WriteBinaryMethod(StringBuilder sb, MethodCallExpression call, string keyWord)
        {
            if (call.Object != null)
            {
                Visit(call.Object, sb);
                sb.Append($" {keyWord} ");
                Visit(call.Arguments[0], sb);
            }
            else
            {
                Visit(call.Arguments[0], sb);
                sb.Append($" {keyWord} ");
                Visit(call.Arguments[1], sb);
            }
        }

        private static bool IsCaseInsensitive(StringComparison strCmp)
        {
            return strCmp == StringComparison.CurrentCultureIgnoreCase ||
                    strCmp == StringComparison.InvariantCultureIgnoreCase ||
                    strCmp == StringComparison.OrdinalIgnoreCase;
        }

        private void TranslateFunc(string func, StringBuilder sb, IEnumerable<Expression> args)
        {
            sb.Append($"{func.ToLowerInvariant()}(");
            var any = false;
            foreach (var callArgument in args)
            {
                any = true;
                Visit(callArgument, sb);
                sb.Append(", ");
            }
            if (any)
                sb.Length -= 2;
            sb.Append(')');
        }

        private void TranslateFunc(string func, StringBuilder sb, params Expression[] args)
        {
            TranslateFunc(func, sb, args.AsEnumerable());
        }


        private void HandleSelect(StringBuilder sb, MethodCallExpression call)
        {
            var args = GetSelectArgs(call.Arguments[1]).ToList();
            if (!args.Any())
                throw new NotImplementedException();

            Visit(call.Arguments[0], sb);

            if (args.Any(a => a.IsAggregation))
            {
                HandleSummarize(sb, args);
                return;
            }

            sb.Append("\n | project ");
            WriteSelectArgs(sb, args);
        }

        private void WriteSelectArgs(StringBuilder sb, List<SelectArg> args)
        {
            var any = false;
            foreach (var arg in args)
            {
                any = true;
                if (arg.IsProject)
                    sb.Append($"{PadFieldName(arg.As)}={PadFieldName(arg.ProjectName)}, ");
                else
                {
                    sb.Append($"{PadFieldName(arg.As)}=");
                    Visit(arg.ExtendExpr, sb);
                    sb.Append(", ");
                }
            }
            if (any)
                sb.Length -= 2;
        }

        private void HandleSummarize(StringBuilder sb, ICollection<SelectArg> args)
        {
            var aggrs = args.Where(sa => sa.IsAggregation).ToList();
            var others = args.Where(sa => !sa.IsAggregation).ToList();
            sb.Append("\n | summarize ");
            WriteSelectArgs(sb, aggrs);
            if (others.Any())
            {
                sb.Append(" by ");
                WriteSelectArgs(sb, others);
            }
        }

        private class SelectArg
        {
            public SelectArg()
            {
                IsAggregation = false;
            }
            public bool IsProject;
            public string ProjectName;
            public string As;
            public Expression ExtendExpr;
            public bool IsAggregation;
        }

        private IBuilder _builder;

        private IEnumerable<SelectArg> GetSelectArgs(Expression expression)
        {
            if (expression is UnaryExpression ue && ue.NodeType == ExpressionType.Quote && ue.Operand is LambdaExpression le)
            {
                if (le.Body is NewExpression ne)
                {
                    var ctorArgs = HandleCtor(ne);
                    var t = GetResultPositions(ctorArgs, new SelectArg[0]);
                    if (_builder == null)
                        _builder = Builder.CreateFromCtor(ne.Constructor, t.ctor);
                    return ctorArgs;
                }
                else if (le.Body is MemberInitExpression mi)
                {
                    var ctorArgs = HandleCtor(mi.NewExpression);
                    var minitArgs = HandleMemberInit(mi.Bindings);

                    var (ctorIds, minitIds) = GetResultPositions(ctorArgs, minitArgs);
                    if (_builder == null)
                        _builder = Builder.CreateFromMemberInit(mi.NewExpression.Constructor, ctorIds,
                            mi.Bindings.Select(bi => bi.Member), minitIds);
                    return ctorArgs.Concat(minitArgs).ToList();
                }
                else if (le.Body is MemberExpression me && me.NodeType == ExpressionType.MemberAccess)
                {
                    var mAccess = HandleMemberAccess(me);
                    if (_builder == null)
                        _builder = Builder.CreateFromMemberAccess(me.Type);
                    return new[]{mAccess};
                }
                else
                {
                    throw new NotImplementedException($"Building results from lambda with Body: {le.Body}");
                }
                //else if (le.Body)
            }
            throw new NotImplementedException($"Build results from '{expression}'");
        }

        private (int[] ctor, int[] minit) GetResultPositions(SelectArg[] ctorArgs, SelectArg[] minitArgs)
        {
            int[] ctorIds;
            int[] minitIds;
            if (!(ctorArgs.Any(a => a.IsAggregation) || minitArgs.Any(a => a.IsAggregation)))
            {
                ctorIds = Enumerable.Range(0, ctorArgs.Length).ToArray();
                minitIds = Enumerable.Range(ctorIds.Length, minitArgs.Length).ToArray();
                return (ctorIds, minitIds);
            }
            ctorIds = new int[ctorArgs.Length];
            minitIds = new int[minitArgs.Length];
            var curIdx = 0;

            void IdSetter(SelectArg[] args, int[] idxes, Func<SelectArg, bool> selector)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    if (selector(arg))
                        idxes[i] = curIdx++;
                }
            }

            IdSetter(ctorArgs, ctorIds, sa => !sa.IsAggregation);
            IdSetter(minitArgs, minitIds, sa => !sa.IsAggregation);
            IdSetter(ctorArgs, ctorIds, sa => sa.IsAggregation);
            IdSetter(minitArgs, minitIds, sa => sa.IsAggregation);
            return (ctorIds, minitIds);
        }

        private SelectArg[] HandleCtor(NewExpression ne)
        {
            var args = new SelectArg[ne.Arguments.Count];
            var parms = ne.Constructor.GetParameters();
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = GetSelectArg(ne.Arguments[i], parms[i].Name);
            }
            return args;
        }

        private SelectArg[] HandleMemberInit(IList<MemberBinding> bindings)
        {
            var args = new SelectArg[bindings.Count];
            for (var i = 0; i < args.Length; i++)
            {
                if (bindings[i] is MemberAssignment binding)
                {
                    args[i] = GetSelectArg(binding.Expression, binding.Member.Name);
                }
                else throw new NotImplementedException();
            }
            return args;
        }

        private SelectArg HandleMemberAccess(MemberExpression me)
        {
            return GetSelectArg(me, me.Member.Name);
        }

        private readonly IsAggregationVisitor _aggregationFinder;
        private SelectArg GetSelectArg(Expression arg, string name)
        {
            if (arg is MemberExpression me)
            {
                return new SelectArg
                {
                    IsProject = true,
                    ProjectName = me.GetMemberName(),
                    As = name
                };
            }
            else
            {
                var selectArg = new SelectArg { IsProject = false, As = name, ExtendExpr = arg };
                selectArg.IsAggregation = _aggregationFinder.UsesAggregationFunction(arg);
                return selectArg;
            }
        }

        private static string GetConstAsAiql(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (obj is DateTime dt)
                return $"datetime({dt:yyyy-MM-ddThh:mm:ssZ})";
            if (obj is TimeSpan ts)
            {
                //TODO make simpler strings for simple cases?
                return $"time({ts})";
            }
            if (obj is string s)
                return AiqlEscape(s);
            return obj.ToString();
        }

        private static string AiqlEscape(string s)
        {
            return JsonConvert.SerializeObject(s);
        }

        private static string GetOperator(ExpressionType exprType)
        {
            switch (exprType)
            {
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.AndAlso:
                    return "and";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
