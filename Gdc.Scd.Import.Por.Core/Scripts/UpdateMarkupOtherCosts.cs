using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateMarkupOtherCosts
    {
        private UpdateCost tpl;

        public UpdateMarkupOtherCosts(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                            .WithTable("Hardware.MarkupOtherCosts")
                            .WithUpdateFields(new string[]{
                                 "Markup",
                                 "MarkupFactor",
                                 "ProlongationMarkup",
                                 "ProlongationMarkupFactor"
                             });
        }

        public string ByCentralContractGroup()
        {
            return tpl.WithDeps(new string[] { "Country", "CentralContractGroup", "ReactionTimeTypeAvailability" }).Build();
        }

        public string ByPla()
        {
            return tpl.WithDeps(new string[] { "Country", "Pla", "ReactionTimeTypeAvailability" }).Build();
        }
    }
}
