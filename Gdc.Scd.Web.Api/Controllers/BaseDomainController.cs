using System;
using System.Collections.Generic;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
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

        public IEnumerable<T> GetAll()
        {
            return this.domainService.GetAll();
        }

        public T Get(Guid id)
        {
            return this.domainService.Get(id);
        }

        public void Save(T item)
        {
            this.domainService.Save(item);
        }

        public void Delete(Guid id)
        {
            this.domainService.Delete(id);
        }
    }
}