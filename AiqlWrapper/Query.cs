using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AiqlWrapper
{
    internal class Query<T> : IQueryable<T>, IOrderedQueryable<T>
    {
        private readonly QueryProvider _provider;

        public Query(QueryProvider provider)
        {
            _provider = provider;
            Expression = Expression.Constant(this);
        }

        public Query(QueryProvider queryProvider, Expression expression)
        {
            _provider = queryProvider;
            Expression = expression;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _provider.ExecuteEnumerable<T>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; }
        public Type ElementType => typeof(T);
        public IQueryProvider Provider => _provider;
    }
}
