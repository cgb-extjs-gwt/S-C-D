using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.TestData.Impl;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.TestData
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
#if DEBUG
            Bind<IConfigureDatabaseHandler>().To<TestDataCreationHandlercs>();
#endif
        }
    }
}
