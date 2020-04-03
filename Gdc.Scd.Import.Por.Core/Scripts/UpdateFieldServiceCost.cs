using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateFieldServiceCost
    {
        private UpdateCost tpl;

        public UpdateFieldServiceCost(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                             .WithTable("Hardware.FieldServiceCost")
                             .WithUpdateFields(new string[] {
                                  "RepairTime",
                                  "TravelTime",
                                  "LabourCost",
                                  "TravelCost",
                                  "PerformanceRate",
                                  "TimeAndMaterialShare",
                                  "OohUpliftFactor"
                            });
        }

        public string ByCentralContractGroup()
        {
            return tpl.WithDeps(new string[] { "Country", "CentralContractGroup", "ServiceLocation", "ReactionTimeType" }).Build();
        }

        public string ByPla()
        {
            return tpl.WithDeps(new string[] { "Country", "Pla", "ServiceLocation", "ReactionTimeType" }).Build();
        }
    }
}
