using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class CostBlockDto : BaseCostBlockMeta<CostElementDto>
    {
        public UsingInfo UsingInfo { get; set; }
    }
}
