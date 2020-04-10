using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
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

        public static IQueryable<TSource> FromSql<TSource>(this IQueryable<TSource> source, string sql, params object[] parameters) where TSource : class
        {
            return RelationalQueryableExtensions.FromSql(source, new RawSqlString(sql), parameters);
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

        public static TSource[] Paging<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit
            )
        {
            return WithPaging(source, start, limit).ToArray();
        }

        public static Task<TSource[]> PagingAsync<TSource>(
               this IQueryable<TSource> source,
               int start,
               int limit
           )
        {
            return GetAsync(WithPaging(source, start, limit));
        }

        public static (TSource[] items, int total) PagingWithCount<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit
            )
        {
            var count = source.Count();
            var result = Paging(source, start, limit);
            return (result, count);
        }

        public static async Task<(TSource[] items, int total)> PagingWithCountAsync<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit
            )
        {
            var count = await GetCountAsync(source);
            var result = await PagingAsync(source, start, limit);
            return (result, count);
        }

        public static Task<TSource> GetSingleAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.SingleAsync(source);
        }

        public static Task<TSource> GetSingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            return EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(source);
        }

        public static IQueryable<TSource> WithPaging<TSource>(
                this IQueryable<TSource> source,
                int start,
                int limit
            )
        {
            return source.Skip(start).Take(limit);
        }

        public static IQueryable<TSource> WhereIf<TSource>(
                this IQueryable<TSource> source,
                bool condition,
                Expression<Func<TSource, bool>> predicate
            )
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IQueryable<TEntity> Include<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath) where TEntity : class
        {
            return EntityFrameworkQueryableExtensions.Include(source, navigationPropertyPath);
        }
    }
}
