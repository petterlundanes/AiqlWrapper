using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AiqlWrapper.Tables
{
    public class AppInsightsTable<T> : IQueryable<T>
    {
        public string QueryName { get; }

        public AppInsightsTable(ApplicationInsightsClient client, string name)
        {
            QueryName = name;
            Provider = new QueryProvider(client);
            Expression = Expression.Constant(this);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();
        }

        public Expression Expression { get; }
        public Type ElementType => typeof(T);
        public IQueryProvider Provider { get; }
    }
}
