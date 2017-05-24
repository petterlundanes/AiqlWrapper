using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AiqlWrapper.Helper
{
    internal class ExpressionsMisc
    {
        public static IQueryable TryGetBaseIQueryable(MethodCallExpression mce)
        {
            while (mce != null)
            {
                if (mce.Arguments.Count == 0)
                    return null;
                if (mce.Arguments[0] is ConstantExpression ce)
                {
                    if (ce.Value is IQueryable iq)
                        return iq;
                    else
                    {
                        return null;
                    }
                }
                mce = mce.Arguments[0] as MethodCallExpression;
            }
            return null;
        }
    }
}
