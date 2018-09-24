using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class QueryableExtensions
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

        public static Task<TSource[]> GetAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.ToArrayAsync(source);
        }

        public static Task<int> GetCountAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.CountAsync(source);
        }

        public static Task<TSource> GetFirstAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.FirstAsync(source);
        }

        public static Task<TSource> GetFirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(source);
        }

        public static IEnumerable<TSource> Paging<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit
            )
        {
            return source.Skip(start).Take(limit).ToList();
        }

        public static IEnumerable<TSource> Paging<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit,
                out int count
            )
        {
            count = source.Count();
            return Paging(source, start, limit);
        }

        public static Task<TSource> GetSingleAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.SingleAsync(source);
        }

        public static Task<TSource> GetSingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source);
        }

        public static IQueryable<TSource> WhereIf<TSource>(
                this IQueryable<TSource> source,
                bool condition,
                Expression<Func<TSource, bool>> predicate
            )
        {
            return condition ? source.Where(predicate) : source;
        }
    }
}
