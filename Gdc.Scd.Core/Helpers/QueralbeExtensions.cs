using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gdc.Scd.Core.Helpers
{
    public static class QueralbeExtensions
    {
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty, bool isDescending)
        {
            var command = isDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var resultExpression = Expression.Call(
                typeof(Queryable),
                command,
                new Type[] { type, property.PropertyType },
                source.Expression,
                Expression.Quote(orderByExpression));

            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }

        public static IQueryable<TSource> WhereIf<TSource>(
                this IQueryable<TSource> source,
                bool condition,
                Expression<Func<TSource, bool>> predicate
            )
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IEnumerable<TSource> Paging<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit,
                out int count
            )
        {
            count = source.Count();
            return source.Skip(start).Take(limit).ToList();
        }
    }
}
