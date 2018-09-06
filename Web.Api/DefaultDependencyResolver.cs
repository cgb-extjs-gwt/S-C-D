using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using System.Web.Mvc;

namespace Web.Api
{
    public class DefaultDependencyResolver : System.Web.Mvc.IDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver, IDependencyScope
    {
        protected IServiceProvider serviceProvider;
        private readonly IServiceScope scope;

        public DefaultDependencyResolver(IServiceProvider serviceProvider)
        {

            this.serviceProvider = serviceProvider;
            this.scope = serviceProvider.CreateScope();
        }

        public IDependencyScope BeginScope()
        {
            return new DefaultDependencyResolver(this.serviceProvider);
        }

        public void Dispose()
        {
            this.scope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.serviceProvider.GetServices(serviceType);
        }
    }

}