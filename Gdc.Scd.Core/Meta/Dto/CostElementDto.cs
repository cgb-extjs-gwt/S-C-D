using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class CostElementDto : BaseCostElementMeta<InputLevelDto>
    {
        public UsingInfo UsingInfo { get; set; }
    }
}
