using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public class UpdateFieldServiceWg
    {
        private UpdateCost tpl;
        public UpdateFieldServiceWg(Wg[] items)
        {
            this.tpl = new UpdateCost(items)
                             .WithTable("Hardware.FieldServiceWg")
                             .WithUpdateFields(new string[] {
                                      "RepairTime"
                            });
        }

        public string ByCentralContractGroup()
        {
            return tpl.WithDeps(new string[] { "CentralContractGroup", "RepairTime" }).Build();
        }

        public string ByPla()
        {
            return tpl.WithDeps(new string[] { "Pla", "RepairTime" }).Build();
        }
    }
}
