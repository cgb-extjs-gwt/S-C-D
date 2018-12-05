using System;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Dto
{
    public class Bundle
    {
        public long Id { get; set; }

        public DateTime EditDate { get; set; }

        public NamedId EditUser { get; set; }

        public int EditItemCount { get; set; }

        public bool IsDifferentValues { get; set; }

        public MetaDto Application { get; set; }

        public NamedId RegionInput { get; set; }

        public MetaDto CostBlock { get; set; }

        public MetaDto CostElement { get; set; }

        public MetaDto InputLevel { get; set; }

        public string QualityGateErrorExplanation { get; set; }
    }
}
