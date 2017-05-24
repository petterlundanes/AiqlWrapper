using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AiqlWrapper.Helper;

namespace AiqlWrapper
{
    public static class QueryExtensions
    {
        private static IOrderedQueryable<TSource> SimplePassThrough<TSource>(IOrderedQueryable<TSource> source, string methodName)
        {
            Expression call = Expression.Call(
                typeof(QueryExtensions).GetMethod(methodName).MakeGenericMethod(typeof(TSource)), source.Expression);
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(call);
        }
        [NoLocalEvaluation]
        public static IOrderedQueryable<TSource> NullsLast<TSource>(this IOrderedQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return SimplePassThrough(source, nameof(NullsLast));
        }

        public static IOrderedQueryable<TSource> NullsFirst<TSource>(this IOrderedQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return SimplePassThrough(source, nameof(NullsFirst));
        }

        private static IOrderedQueryable<TSource> CreateTopExpression<TSource, TKey>(IQueryable<TSource> source,
            int takeRows, Expression<Func<TSource, TKey>> keySelector, string methodName)
        {
            Expression call = Expression.Call(
                typeof(QueryExtensions).GetMethod(methodName).MakeGenericMethod(typeof(TSource), typeof(TKey)),
                source.Expression, Expression.Constant(takeRows), keySelector);
            var queryable = (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>(call);
            return queryable;
        }

        public static IOrderedQueryable<TSource> Top<TSource, TKey>(this IQueryable<TSource> source, int takeRows,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return CreateTopExpression(source, takeRows, keySelector, nameof(Top));
        }
        public static IOrderedQueryable<TSource> TopDescending<TSource, TKey>(this IQueryable<TSource> source, int takeRows,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return CreateTopExpression(source, takeRows, keySelector, nameof(TopDescending));
        }
    }
}
