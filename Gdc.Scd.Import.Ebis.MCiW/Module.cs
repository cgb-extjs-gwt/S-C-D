using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Import.Core.Interfaces;
using Gdc.Scd.Spooler.Core;
using NLog;

namespace Gdc.Scd.Import.Ebis.MCiW
{
    public class Module : BaseJobModule<Module>
    {
        public override void Load()
        {
            Bind(typeof(IRepository<MaterialCostWarrantyEmeia>)).To(typeof(ImportRepository<MaterialCostWarrantyEmeia>)).InSingletonScope();
            Bind<ILogger<LogLevel>, Gdc.Scd.Core.Interfaces.ILogger>().To<Core.Impl.Logger>().InSingletonScope();

            Bind<IDownloader>().To<FileDownloader>().InSingletonScope();
            Bind(typeof(IParser<>)).To(typeof(Parser<>)).InSingletonScope();
            Bind(typeof(IUploader<>)).To(typeof(EbisMaterialCostUploader)).InSingletonScope();
            Bind<IImportManager>().To<FileImportManager<MaterialCostDto>>().InSingletonScope();
            Bind<IConfigHandler>().To<DataBaseConfigHandler>().InSingletonScope();

            Bind<MaterialCostService>().ToSelf();
        }
    }
}
