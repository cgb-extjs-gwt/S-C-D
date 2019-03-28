using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Ninject;
using Ninject.Modules;
using NLog;
using System.Configuration;

namespace Gdc.Scd.Import.CentralContractGroup
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ImportRepository<Wg>>().ToSelf().InSingletonScope();
            Bind<ILogger<LogLevel>>().To<Core.Impl.Logger>().InSingletonScope();
            Bind<IDataAccessManager>().To<SqlManager>()
                .InSingletonScope()
                .WithConstructorArgument("connectionString", 
                ConfigurationManager.ConnectionStrings["Partner_New"].ConnectionString);
            Bind<IDataImporter<CentralContractGroupDto>>().To<CentralContractGroupImporter>().InSingletonScope();
            Bind<IImportManager>().To<DbImportManager<CentralContractGroupDto>>().InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(CentralContractGroupUploader)).InSingletonScope();

            this.Bind<Scd.Core.Interfaces.IPrincipalProvider>().To<ConsolePrincipleProvider>().InSingletonScope(); 
        }
    }
}
