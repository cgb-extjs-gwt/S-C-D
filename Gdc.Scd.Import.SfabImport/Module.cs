﻿using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Ninject;
using Ninject.Modules;
using NLog;

namespace Gdc.Scd.Import.SfabImport
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ImportRepository<Wg>>().ToSelf().InSingletonScope();
            Bind(typeof(IRepository<Sog>)).To(typeof(ImportRepository<Sog>)).InSingletonScope();
            Bind(typeof(IRepository<SFab>)).To(typeof(ImportRepository<SFab>)).InSingletonScope();
            
            Bind<ILogger<LogLevel>, Gdc.Scd.Core.Interfaces.ILogger>().To<Core.Impl.Logger>().InSingletonScope();

            Bind<IDownloader>().To<FileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(SfabUploader)).InSingletonScope();
            Bind<IImportManager>().To<FileImportManager<SFabDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<DataBaseConfigHandler>().InSingletonScope();
            
            Bind<Scd.Core.Interfaces.IPrincipalProvider>().To<ConsolePrincipleProvider>().InSingletonScope();

            Bind<SFabService>().ToSelf();
        }

        public static StandardKernel CreateKernel()
        {
            NinjectExt.IsConsoleApplication = true;
            return new StandardKernel(
                new Scd.Core.Module(),
                new Scd.DataAccessLayer.Module(),
                new Scd.BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
