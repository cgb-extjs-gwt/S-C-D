﻿using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Dto
{
    public class CostElementDto : BaseCostElementMeta<InputLevelDto>
    {
        public bool IsUsingCostEditor { get; set; }

        public bool IsUsingTableView { get; set; }

        public bool IsUsingCostImport { get; set; }
    }
}
