using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateFieldServiceReactionTimeType
    {
        private UpdateCost tpl;
        public UpdateFieldServiceReactionTimeType(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                             .WithTable("Hardware.FieldServiceReactionTimeType")
                             .WithUpdateFields(new string[] {
                                      "PerformanceRate",
                                      "TimeAndMaterialShare"
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
