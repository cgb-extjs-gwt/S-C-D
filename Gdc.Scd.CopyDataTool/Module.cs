using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.CopyDataTool.Configuration;
using Ninject;
using Ninject.Modules;

namespace Gdc.Scd.CopyDataTool
{
    public class Module : NinjectModule
    {

        public override void Load()
        {
            this.Bind<CopyDetailsConfig>()
                .ToConstant((CopyDetailsConfig) ConfigurationManager.GetSection("copyDetailsConfig"));
        }
    }
}
