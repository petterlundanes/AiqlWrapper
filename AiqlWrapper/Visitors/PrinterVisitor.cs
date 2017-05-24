using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Helper;

namespace AiqlWrapper.Visitors
{
    public class PrinterVisitor : ExpressionVisitor
    {
        private readonly TextWriter _writer;

        public PrinterVisitor()
        {
            _writer = Console.Out;
        }
        public PrinterVisitor(TextWriter writer)
        {
            _writer = writer;
        }
        private int _lvl = 0;
        public override Expression Visit(Expression node)
        {
            if (node != null)
                _writer.WriteLine($"{new string('\t', _lvl)}{node.NodeType} {node.GetHashCode():X}  {GetExtra(node)}");
            _lvl++;
            var tmp = base.Visit(node);
            _lvl--;
            return tmp;
        }
        private static string GetExtra(Expression expr)
        {
            if (expr is MethodCallExpression mce)
            {
                TranslateFuncAttribute translate;
                var add = "";
                if ((translate = mce.Method.GetCustomAttribute<TranslateFuncAttribute>()) != null &&
                    translate.IsAggregation)
                    add = " (AGGR)";
                return mce.Method.Name + add;
            }
            if (expr is MemberExpression me)
                return me.Member.Name;
            if (expr is ParameterExpression pe)
                return pe.Name;
            if (expr is ConstantExpression ce)
            {
                if (ce.Value == null) return "null";
                return ce.Value.ToString();
            }
            return "";
        }
    }
}
