using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Entities
{
    public class TableViewCostBlockInfo
    {
        public CostBlockEntityMeta Meta { get; set; }

        public IEnumerable<string> CostElementIds { get; set; }
    }
}
