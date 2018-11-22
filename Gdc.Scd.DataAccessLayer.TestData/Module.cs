using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.TestData.Impl;
using Ninject.Modules;

namespace Gdc.Scd.DataAccessLayer.TestData
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IConfigureDatabaseHandler>().To<TestDataCreationHandlercs>();
        }
    }
}
