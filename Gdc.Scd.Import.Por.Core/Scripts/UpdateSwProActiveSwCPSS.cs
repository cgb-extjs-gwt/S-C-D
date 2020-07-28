using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateSwProActiveSwCPSS
    {
        private UpdateCost tpl;

        public UpdateSwProActiveSwCPSS(SwDigit[] items)
        {
            this.tpl = new UpdateSwCost(items)
                            .WithTable("SoftwareSolution.ProActiveSwCPSS")
                            .WithUpdateFields(new string[] {
                                "LocalRemoteAccessSetupPreparationEffort",
                                "LocalRegularUpdateReadyEffort",
                                "LocalPreparationShcEffort",
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
