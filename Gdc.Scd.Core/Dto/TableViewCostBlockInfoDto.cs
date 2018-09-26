using System.Collections.Generic;

namespace Gdc.Scd.Core.Dto
{
    public class TableViewCostBlockInfoDto
    {
        public string MetaId { get; set; }

        public IEnumerable<string> CostElementIds { get; set; }
    }
}
