using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Meta.Entities
{
    public class DomainMeta
    {
        public IEnumerable<CostBlockMeta>  CostBlocks { get; set; }

        public IEnumerable<InputLevelMeta> InputLevels { get; set; }

        public IEnumerable<ApplicationMeta> Applications { get; set; }

        public IEnumerable<ScopeMeta> Scopes { get; set; }

        public CostBlockMeta GetCostBlock(string id)
        {
            return this.CostBlocks.FirstOrDefault(costBlock => costBlock.Id == id);
        }
    }
}
