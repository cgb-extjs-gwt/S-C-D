using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.Web.Server.Entities;
using Gdc.Scd.Web.Server.Impl;
using Ninject.Modules;

namespace Gdc.Scd.Web.Server
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPrincipalProvider>().To<PrincipalProvider>().InSingletonScope();
            this.Bind<RequestInfoFilter>().To<RequestInfoFilter>().InSingletonScope();
            Kernel.RegisterEntity<RequestInfo>();
        }
    }
}