using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateSwProactive
    {
        private UpdateCost tpl;

        public UpdateSwProactive(SwDigit[] items)
        {
            this.tpl = new UpdateSwCost(items)
                .WithTable("SoftwareSolution.ProActiveSw")
                .WithUpdateFields(new string[] {
                    "LocalRemoteAccessSetupPreparationEffort",
                    "LocalRegularUpdateReadyEffort",
                    "LocalPreparationShcEffort",
                    "CentralExecutionShcReportCost",
                    "LocalRemoteShcCustomerBriefingEffort",
                    "LocalOnSiteShcCustomerBriefingEffort",
                    "TravellingTime",
                    "OnSiteHourlyRate"
                });
        }
        public string BySog()
        {
            return tpl.WithDeps(new string[] { "Country", "Sog" }).Build();
        }
    }
}
