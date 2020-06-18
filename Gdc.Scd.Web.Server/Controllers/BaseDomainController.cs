using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.Web.Server.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public abstract class BaseDomainController<T> : ApiController where T : IIdentifiable
    {
        protected readonly IDomainService<T> domainService;

        public BaseDomainController(IDomainService<T> domainService)
        {
            this.domainService = domainService;
        }

        public virtual Task<T[]> GetAll()
        {
            return this.domainService.GetAll().GetAsync();
        }

        public virtual async Task<DataInfo<T>> GetBy(int start, int limit, string sort = null, string filter = null)
        {
            var allItems = this.domainService.GetAll();
            var query = allItems.Skip(start).Take(limit);
            var sortInfos = sort == null ? null : JsonConvert.DeserializeObject<SortInfo[]>(sort);
            var filterInfos = filter == null ? null : JsonConvert.DeserializeObject<Entities.FilterInfo[]>(sort);

            query = this.OrderBy(query, sortInfos);
            query = this.Filter(query, filterInfos);

            return new DataInfo<T>
            {
                Items = await query.GetAsync(),
                Total = await this.Filter(allItems, filterInfos).GetCountAsync()
            };
        }

        public virtual T Get(long id)
        {
            return this.domainService.Get(id);
        }

        [HttpPost]
        public virtual void Save([FromBody]T item)
        {
            this.domainService.Save(item);
        }

        [HttpPost]
        public virtual void SaveAll([FromBody]IEnumerable<T> items)
        {
            this.domainService.Save(items);
        }

        [HttpPost]
        public virtual void Delete([FromBody]long id)
        {
            this.domainService.Delete(id);
        }

        [HttpPost]
        public virtual HttpResponseMessage DeleteAll([FromBody]IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.domainService.Delete(item.Id);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected virtual IQueryable<T> OrderBy(IQueryable<T> query, SortInfo[] sortInfos)
        {
            if (sortInfos != null)
            {
                for (var i = 0; i < sortInfos.Length; i++)
                {
                    var sortInfo = sortInfos[i];
                    query = QueryableExtensions.OrderBy(query, sortInfo.Property, sortInfo.Direction == SortDirection.Desc);
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