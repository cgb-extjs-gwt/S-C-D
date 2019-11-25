using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateProactive
    {
        private UpdateCost tpl;

        public UpdateProactive(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                            .WithTable("Hardware.ProActive")
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

        public string ByCentralContractGroup()
        {
            return tpl.WithDeps(new string[] { "Country", "CentralContractGroup" }).Build();
        }

        public string ByPla()
        {
            return tpl.WithDeps(new string[] { "Country", "Pla" }).Build();
        }
    }
}
