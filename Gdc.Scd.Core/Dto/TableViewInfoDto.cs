using System.Collections.Generic;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.Core.Entities
{
    public class TableViewInfoDto
    {
        public IEnumerable<TableViewCostBlockInfoDto> CostBlockInfos { get; set; }

        public IDictionary<string, IEnumerable<NamedId>> Filters { get; set; }
    }
}
