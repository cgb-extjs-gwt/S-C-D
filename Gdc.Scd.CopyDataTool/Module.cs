using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Ninject.Modules;

namespace Gdc.Scd.CopyDataTool
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepositorySet, IRegisteredEntitiesProvider, EntityFrameworkRepositorySet>()
                .To<EntityFrameworkRepositorySet>()
                .When(r => true)
                .InScdRequestScope()
                .WithConstructorArgument("connectionStringName", "SourceDB");
        }
    }
}
