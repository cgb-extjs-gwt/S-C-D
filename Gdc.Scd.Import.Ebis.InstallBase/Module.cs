using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Ninject.Modules;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Ebis.InstallBase
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IRepository<Gdc.Scd.Core.Entities.InstallBase>)).To(typeof(ImportRepository<Gdc.Scd.Core.Entities.InstallBase>)).InSingletonScope();
            Bind(typeof(IRepository<>)).To(typeof(EntityFrameworkRepository<>)).InSingletonScope();
            Bind<ISqlRepository>().To<SqlRepository>().InSingletonScope();
            Bind<ILogger<LogLevel>>().To<Core.Impl.Logger>().InSingletonScope();

            Bind<IDownloader>().To<FileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(InstallBaseUploader)).InSingletonScope();
            Bind<IImportManager>().To<ImportManager<InstallBaseDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<DataBaseConfigHandler>().InSingletonScope();

            Kernel.RegisterEntity<Country>();
            Kernel.RegisterEntity<ImportConfiguration>();
            Kernel.RegisterEntity<Wg>();
            Kernel.RegisterEntity<Pla>();
            Kernel.RegisterEntity<Gdc.Scd.Core.Entities.InstallBase>();
        }
    }
}
