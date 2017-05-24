using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Helper;

namespace AiqlWrapper.Visitors
{
    internal class IsAggregationVisitor : ExpressionVisitor
    {
        public bool UsesAggregationFunction(Expression expr)
        {
            _usesAggr = false;
            Visit(expr);
            return _usesAggr;
        }

        private bool _usesAggr;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            TranslateFuncAttribute translate;
            if ((translate = node.Method.GetCustomAttribute<TranslateFuncAttribute>()) != null &&
                translate.IsAggregation)
            {
                _usesAggr = true;
                return node;
            }
            return base.VisitMethodCall(node);
}
}
}
