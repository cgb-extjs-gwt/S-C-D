using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Ninject;
using Ninject.Modules;
using NLog;

namespace Gdc.Scd.Import.Logistics
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ILogger<LogLevel>, Gdc.Scd.Core.Interfaces.ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();
            Bind<IDownloader>().To<FileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(LogisticUploader)).InSingletonScope();
            Bind<IImportManager>().To<FileImportManager<LogisticsDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<DataBaseConfigHandler>().InSingletonScope();
            Bind<LogisticsImportService>().To<LogisticsImportService>();
        }

        public static StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Scd.Core.Module(),
                new Scd.DataAccessLayer.Module(),
                new Scd.BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
