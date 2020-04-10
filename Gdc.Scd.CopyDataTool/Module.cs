using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.Core.Interfaces;
using Ninject;
using Ninject.Modules;
using System.Configuration;

namespace Gdc.Scd.CopyDataTool
{
    public class Module : NinjectModule
    {

        public override void Load()
        {
            this.Bind<CopyDetailsConfig>()
                .ToConstant((CopyDetailsConfig) ConfigurationManager.GetSection("copyDetailsConfig"));

            this.Bind<IPrincipalProvider>().To<DataCopyPrincipleProvider>().WithConstructorArgument("user",
                Kernel.Get<CopyDetailsConfig>().EditUser);

            this.Bind<CopyDataToolHelperService>().To<CopyDataToolHelperService>().InSingletonScope();
        }
    }
}
