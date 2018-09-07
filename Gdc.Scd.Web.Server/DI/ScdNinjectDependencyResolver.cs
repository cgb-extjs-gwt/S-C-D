using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
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
using Gdc.Scd.Web.Server.Service;
using Ninject.Web.WebApi;

namespace Gdc.Scd.Web.Server.DI
{
    public class ScdNinjectDependencyResolver : ScdNinjectDependencyScope,
        System.Web.Mvc.IDependencyResolver,
        System.Web.Http.Dependencies.IDependencyResolver
    {
        private IKernel kernel;

        public ScdNinjectDependencyResolver(IKernel kernel)
            :base(kernel)
        {
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(this.kernel.BeginBlock());
        }

        private void AddBindings()
        {
           
        }
    }
}