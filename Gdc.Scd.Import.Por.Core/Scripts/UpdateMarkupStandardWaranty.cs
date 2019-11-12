using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateMarkupStandardWaranty
    {
        private UpdateCost tpl;

        public UpdateMarkupStandardWaranty(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                            .WithTable("Hardware.MarkupStandardWaranty")
                            .WithUpdateFields(new string[] {
                                "MarkupStandardWarranty",
                                "MarkupFactorStandardWarranty"
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
