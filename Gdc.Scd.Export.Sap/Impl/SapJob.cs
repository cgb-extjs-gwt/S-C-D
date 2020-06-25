using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.Sap.Interfaces;
using Gdc.Scd.Spooler.Core.Entities;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject;

namespace Gdc.Scd.Export.Sap.Impl
{
    public class SapJob : IJob
    {
        private readonly IManualCostExportService manualCostExportService;
        public const string JobName = "SapJob";

        public SapJob()
        {
            var kernel = Module.CreateKernel();
            this.manualCostExportService = kernel.Get<IManualCostExportService>();
        }

        public IOperationResult Output()
        {
            this.manualCostExportService.Export();

            return new OperationResult<bool>
            {
                IsSuccess = true,
                Result = true
            };
        }

        public string WhoAmI()
        {
            return "SapJob";
        }
    }
}
