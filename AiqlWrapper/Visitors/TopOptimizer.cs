using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Helper;

namespace AiqlWrapper.Visitors
{
    internal class TopOptimizer : ExpressionVisitor
    {
        public static Expression Optimize(Expression expr, out bool change)
        {
            var visitor = new TopOptimizer();
            var res = visitor.Visit(expr);
            change = visitor._change;
            return res;
        }

        private bool _change;

        public TopOptimizer()
        {
            _change = false;
        }

        private MethodCallExpression _takeExpr;
        private bool _validSearch = false;
        public override Expression Visit(Expression node)
        {
            if (node == null) return null;
            if (node is MethodCallExpression mce)
            {
                if (mce.Method.Name == nameof(Queryable.Take))
                {
                    if (_takeExpr != null)
                        throw new NotSupportedException("multiple Take calls are not currently supported.");
                    _takeExpr = mce;
                    _validSearch = true;
                    var expr = base.Visit(node);
                    if (_takeExpr != null)
                        return expr;
                    if (expr is MethodCallExpression retMce)
                        return retMce.Arguments[0];
                    else
                        throw new Exception("should never happen");
                }
                else if (mce.Method.Name.StartsWith(nameof(Queryable.OrderBy)))
                {
                    if (_takeExpr == null || !_validSearch)
                    {
                        return base.Visit(node);
                    }
                    else
                    {
                        var take = _takeExpr;
                        _takeExpr = null;
                        _validSearch = false;
                        base.Visit(node);
                        _change = true;
                        var method = mce.Method.Name.Contains(Constants.DescendingPostfix)
                            ? nameof(QueryExtensions.TopDescending)
                            : nameof(QueryExtensions.Top);
                        var keySelector = mce.Arguments[1];
                        // KeySelector type = Func<TSource,TKey>
                        var funcType = keySelector.Type.GenericTypeArguments[0];
                        var tSource = funcType.GenericTypeArguments[0];
                        var tKey = funcType.GenericTypeArguments[1];
                        // source rows, keySelector
                        return Expression.Call(null,
                            typeof(QueryExtensions).GetMethod(method).MakeGenericMethod(tSource, tKey),
                            mce.Arguments[0], take.Arguments[1], keySelector);
                    }
                }
                else if (_validSearch && (mce.Method.Name.StartsWith(nameof(Queryable.Where))))
                {
                    _validSearch = false;
                }

                return base.Visit(node);
            }
            else
            {
                _validSearch = false;
                return base.Visit(node);
            }
        }
    }
}
