using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.TestData.Impl;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Ninject.Web.Common;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Server.DI
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        readonly IKernel kernal;

        public NinjectDependencyResolver(IKernel kernel)
        {
            this.kernal = kernel;
        }

        public object GetService(Type serviceType)
        {
            return kernal.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernal.GetAll(serviceType);
        }
    }
}