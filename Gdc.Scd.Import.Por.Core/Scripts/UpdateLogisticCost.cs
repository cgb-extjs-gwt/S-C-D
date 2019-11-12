using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateLogisticCost : UpdateCost
    {
        public UpdateLogisticCost(Wg[] items) : base(items)
        {
            this.table = "Hardware.LogisticsCosts";
            this.updateFields = new string[] {
                  "StandardHandling",
                  "HighAvailabilityHandling",
                  "StandardDelivery",
                  "ExpressDelivery",
                  "TaxiCourierDelivery",
                  "ReturnDeliveryFactory"
            };
        }

        public string ByCentralContractGroup()
        {
            this.deps = new string[] { "Country", "CentralContractGroup", "ReactionTimeType" };
            return Build();
        }

        public string ByPla()
        {
            this.deps = new string[] { "Country", "Pla", "ReactionTimeType" };
            return Build();
        }
    }
}
