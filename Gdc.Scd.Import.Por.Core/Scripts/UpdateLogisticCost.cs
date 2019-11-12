using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateLogisticCost 
    {
        private UpdateCost tpl;

        public UpdateLogisticCost(Wg[] items) 
        {
            this.tpl = new UpdateCost(items)
                             .WithTable("Hardware.LogisticsCosts")
                             .WithUpdateFields(new string[] {
                                  "StandardHandling",
                                  "HighAvailabilityHandling",
                                  "StandardDelivery",
                                  "ExpressDelivery",
                                  "TaxiCourierDelivery",
                                  "ReturnDeliveryFactory"
                            });
        }

        public string ByCentralContractGroup()
        {
            return tpl.WithDeps(new string[] { "Country", "CentralContractGroup", "ReactionTimeType" }).Build();
        }

        public string ByPla()
        {
            return tpl.WithDeps(new string[] { "Country", "Pla", "ReactionTimeType" }).Build();
        }
    }
}
