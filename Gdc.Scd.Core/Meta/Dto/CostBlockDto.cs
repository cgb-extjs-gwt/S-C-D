using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class CostBlockDto : BaseCostBlockMeta<CostElementDto>
    {
        public bool IsUsingCostEditor { get; set; }

        public bool IsUsingTableView { get; set; }
    }
}
