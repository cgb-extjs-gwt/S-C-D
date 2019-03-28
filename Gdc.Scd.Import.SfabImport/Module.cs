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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.SfabImport
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ImportRepository<Wg>>().ToSelf().InSingletonScope();
            Bind(typeof(IRepository<Sog>)).To(typeof(ImportRepository<Sog>)).InSingletonScope();
            Bind(typeof(IRepository<SFab>)).To(typeof(ImportRepository<SFab>)).InSingletonScope();
            
            Bind<ILogger<LogLevel>>().To<Core.Impl.Logger>().InSingletonScope();

            Bind<IDownloader>().To<FileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(SfabUploader)).InSingletonScope();
            Bind<IImportManager>().To<FileImportManager<SFabDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<DataBaseConfigHandler>().InSingletonScope();
            
            Bind<Scd.Core.Interfaces.IPrincipalProvider>().To<ConsolePrincipleProvider>().InSingletonScope();
        }
    }
}
