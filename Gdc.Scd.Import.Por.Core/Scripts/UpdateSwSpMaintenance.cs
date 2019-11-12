using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateSwSpMaintenance
    {
        private UpdateCost tpl;

        public UpdateSwSpMaintenance(SwDigit[] items)
        {
            this.tpl = new UpdateSwCost(items)
                            .WithTable("SoftwareSolution.SwSpMaintenance")
                            .WithUpdateFields(new string[] {
                                "2ndLevelSupportCosts",
                                "ReinsuranceFlatfee",
                                "CurrencyReinsurance",
                                "RecommendedSwSpMaintenanceListPrice",
                                "MarkupForProductMarginSwLicenseListPrice",
                                "ShareSwSpMaintenanceListPrice",
                                "DiscountDealerPrice"
                            });
        }

        public string BySog()
        {
            return tpl.WithDeps(new string[] { "Sog", "DurationAvailability" }).Build();
        }
    }
}
