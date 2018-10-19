using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class ApplicationDto : BaseMeta
    {
        public bool IsUsingCostEditor { get; set; }

        public bool IsUsingTableView { get; set; }
    }
}
