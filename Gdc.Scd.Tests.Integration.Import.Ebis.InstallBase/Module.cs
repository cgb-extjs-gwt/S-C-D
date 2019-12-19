using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Gdc.Scd.Import.Ebis.InstallBase;
using Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase.Fakes;
using Ninject.Modules;
using NLog;

namespace Gdc.Scd.Tests.Integration.Import.Ebis.InstallBase
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IRepository<Gdc.Scd.Core.Entities.InstallBase>)).To(typeof(ImportRepository<Gdc.Scd.Core.Entities.InstallBase>)).InSingletonScope();
            Bind<ILogger<LogLevel>>().To<Gdc.Scd.Import.Core.Impl.Logger>().InSingletonScope();

            Bind<IDownloader>().To<FakeFileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(InstallBaseUploader)).InSingletonScope();
            Bind<IImportManager>().To<FileImportManager<InstallBaseDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<FakeDataBaseConfigHandler>().InSingletonScope();

            Bind<InstallBaseService>().ToSelf();
        }
    }
}
