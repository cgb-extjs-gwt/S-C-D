using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateFieldServiceLocation
    {
            private UpdateCost tpl;

            public UpdateFieldServiceLocation(Wg[] items)
            {
                this.tpl = new UpdateCost(items)
                                 .WithTable("Hardware.FieldServiceLocation")
                                 .WithUpdateFields(new string[] {
                                      "TravelTime",
                                      "LabourCost",
                                      "TravelCost"
                                });
            }

            public string ByCentralContractGroup()
            {
                return tpl.WithDeps(new string[] { "Country", "CentralContractGroup", "ServiceLocation" }).Build();
            }

            public string ByPla()
            {
                return tpl.WithDeps(new string[] { "Country", "Pla", "ServiceLocation" }).Build();
            }
    }
}
