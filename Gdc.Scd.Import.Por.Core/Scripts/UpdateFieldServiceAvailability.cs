using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateFieldServiceAvailability
    {
        private UpdateCost tpl;

        public UpdateFieldServiceAvailability(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                             .WithTable("Hardware.FieldServiceAvailability")
                             .WithUpdateFields(new string[] {
                                      "OohUpliftFactor"
                            });
        }

        public string ByCentralContractGroup()
        {
            return tpl.WithDeps(new string[] { "Country", "CentralContractGroup", "Availability" }).Build();
        }

        public string ByPla()
        {
            return tpl.WithDeps(new string[] { "Country", "Pla", "Availability" }).Build();
        }
    }
}
