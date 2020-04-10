using Gdc.Scd.Core.Helpers;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.Spooler.Core
{
    public abstract class BaseJobModule<TModule> : NinjectModule
        where TModule : BaseJobModule<TModule>, new()
    {
        public static StandardKernel CreateKernel()
        {
            NinjectExt.IsConsoleApplication = true;

            return new StandardKernel(
                new TModule(),
                new Scd.BusinessLogicLayer.Module(),
                new Scd.DataAccessLayer.Module(),
                new Scd.Core.Module());
        }
    }
}
