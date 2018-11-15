﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IQualityGateSevice
    {
        Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, CostEditorContext context);

        Task<QualityGateResult> Check(CostBlockHistory history);

        bool IsUseCheck(ICostElementIdentifier context);
    }
}
