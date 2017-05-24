using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AiqlWrapper.Helper;

namespace AiqlWrapper.Visitors
{
    internal static class LocalExpressionEvaluator
    {
        public static Expression Evaluate(Expression expression)
        {
            var set = LocalEvaluationFinder.Search(expression);
            return LocalEvaluator.Evaluate(expression, set);
        }
        private class LocalEvaluationFinder : ExpressionVisitor
        {
            public static HashSet<Expression> Search(Expression expression)
            {
                var searcher = new LocalEvaluationFinder();
                searcher.Visit(expression);
                return searcher._resultSet;
            }
            private LocalEvaluationFinder()
            {
                _resultSet = new HashSet<Expression>();
                _aiqlParameters = new HashSet<ParameterExpression>();
            }

            private readonly HashSet<Expression> _resultSet;
            private bool _invalidSubtree;
            private readonly HashSet<ParameterExpression> _aiqlParameters;

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return null;

                if (node is MethodCallExpression mc)
                {
                    var iq = ExpressionsMisc.TryGetBaseIQueryable(mc);

                    if (iq != null && iq.Provider.GetType() == typeof(QueryProvider))
                    {
                        foreach (var expr in mc.Arguments.GetExpressions<UnaryExpression>(ExpressionType.Quote))
                        {
                            if (expr.Operand is LambdaExpression la)
                            {
                                foreach (var para in la.Parameters)
                                {
                                    _aiqlParameters.Add(para);
                                    Console.WriteLine($"Added aiql parameter: {para.Name} - {para.GetHashCode():X}");
                                }
                            }
                        }
                    }
                }

                var save = _invalidSubtree;
                _invalidSubtree = false;

                if (node.NodeType == ExpressionType.Parameter && _aiqlParameters.Contains(node))
                    _invalidSubtree = true;
                if (node is MethodCallExpression mce && 
                    (mce.Method.CustomAttributes.Any(ca => ca.AttributeType == typeof(NoLocalEvaluationAttribute))
                    || (mce.Arguments.Count >= 1 && mce.Arguments[0] is ConstantExpression ce && ce.Value is IQueryable q && q.Provider.GetType() == typeof(QueryProvider)))
                    || node is NewExpression)
                {
                    _invalidSubtree = true;
                }

                var res = base.Visit(node);
                if (!_invalidSubtree)
                    _resultSet.Add(node);

                _invalidSubtree |= save;

                return res;
            }
        }

        private class LocalEvaluator : ExpressionVisitor
        {
            public static Expression Evaluate(Expression root, HashSet<Expression> locals)
            {
                var visitor = new LocalEvaluator(locals);
                return visitor.Visit(root);
            }
            private readonly HashSet<Expression> _locals;

            private LocalEvaluator(HashSet<Expression> locals)
            {
                _locals = locals;
            }

            public override Expression Visit(Expression node)
            {
                if (_locals.Contains(node))
                {
                    if (node.NodeType == ExpressionType.Constant)
                        return node;

                    var val = Expression.Lambda(node).Compile().DynamicInvoke(null);
                    return Expression.Constant(val, node.Type);
                }
                else
                    return base.Visit(node);
            }
        }
    }
}
