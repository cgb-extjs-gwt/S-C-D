using Ninject;
using System;
using System.Collections.Generic;
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