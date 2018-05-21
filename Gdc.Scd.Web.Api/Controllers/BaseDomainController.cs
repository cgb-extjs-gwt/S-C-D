using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Controllers
{
    public abstract class BaseDomainController<T> : Controller where T : IIdentifiable
    {
        protected readonly IDomainService<T> domainService;

        public BaseDomainController(IDomainService<T> domainService)
        {
            this.domainService = domainService;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this.domainService.GetAll();
        }

        public virtual IEnumerable<T> GetBy(int start, int limit, SortInfo[] sort = null, FilterInfo[] filter = null)
        {
            var query = this.domainService.GetAll().Skip(start).Take(limit);

            query = this.OrderBy(query, sort);
            query = this.Filter(query, filter);

            return query;
        }

        public virtual T Get(Guid id)
        {
            return this.domainService.Get(id);
        }

        public virtual void Save(T item)
        {
            this.domainService.Save(item);
        }

        public virtual void Delete(Guid id)
        {
            this.domainService.Delete(id);
        }

        protected virtual IQueryable<T> OrderBy(IQueryable<T> query, SortInfo[] sortInfos)
        {
            if (sortInfos != null && sortInfos.Length > 0)
            {
                foreach (var sortInfo in sortInfos)
                {
                    query = query.OrderBy(sortInfo.Property, sortInfo.Direction == SortDirection.Desc);
                }
            }

            return query;
        }

        protected virtual IQueryable<T> Filter(IQueryable<T> query, FilterInfo[] filterInfos)
        {
            if (filterInfos != null && filterInfos.Length > 0)
            {
                var param = Expression.Parameter(typeof(T), "item");
                var exp = GetEqualExpression(filterInfos[0], param);

                for (var i = 1; i < filterInfos.Length; i++)
                {
                    exp = Expression.Or(exp, GetEqualExpression(filterInfos[i], param));
                }

                query = query.Where(Expression.Lambda<Func<T, bool>>(exp, new[] { param }));
            }

            return query;

            BinaryExpression GetEqualExpression(FilterInfo filterInfo, Expression param)
            {
                return Expression.Equal(
                    Expression.Property(param, filterInfo.Property),
                    Expression.Constant(this.ConvertToValue(filterInfo)));
            }
        }

        protected virtual object ConvertToValue(FilterInfo filterInfo)
        {
            var type = typeof(T);
            var property = type.GetProperty(filterInfo.Property);

            return Convert.ChangeType(filterInfo.Value, property.PropertyType);
        }
    }
}