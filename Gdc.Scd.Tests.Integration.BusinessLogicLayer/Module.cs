using Gdc.Scd.Core.Helpers;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class Module : NinjectModule
    {
        public override void Load() { }

        public static StandardKernel CreateKernel()
        {
            NinjectExt.IsConsoleApplication = true;
            return new StandardKernel(
                new Module(),
                new Gdc.Scd.BusinessLogicLayer.Module(),
                new Gdc.Scd.DataAccessLayer.Module { ExcludeModifiableDecoratorRepository = true },
                new Scd.Core.Module());
        }
    }
}
