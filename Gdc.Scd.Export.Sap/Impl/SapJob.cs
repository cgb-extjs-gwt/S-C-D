using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.Sap.Interfaces;
using Gdc.Scd.Spooler.Core.Entities;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject;
using System;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.Sap.Enitities;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class SapJob : IJob
    {
        private readonly IManualCostExportService manualCostExportService;
        public const string JobName = "SapJob";
        protected ILogger logger;

        public SapJob()
        {
            var kernel = Module.CreateKernel();
            logger = kernel.Get<ILogger>();
            this.manualCostExportService = new ManualCostExportService(kernel.Get<IRepository<HardwareManualCost>>(),
                kernel.Get<IRepository<User>>(), kernel.Get<IDomainService<SapTable>>(), kernel.Get<ISapExportLogService>(),
                kernel.Get<IRepositorySet>(), logger);

            logger.Info(SapLogConstants.INITIALIZATION_END);
        }

        public IOperationResult Output()
        {
            try
            {
                this.manualCostExportService.Export();
                return Result(true);
            }
            catch (Exception e)
            {
                logger.Fatal(e, SapLogConstants.UNEXPECTED_ERROR);
                return Result(false);
            }

            
        }

        public string WhoAmI() => JobName;

        public static OperationResult<bool> Result(bool ok)
        {
            return new OperationResult<bool> { IsSuccess = ok, Result = true };
        }
    }
}
