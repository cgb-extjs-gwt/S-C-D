using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Meta.Entities;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostEditorService
    {
        Task<IEnumerable<string>> GetCostElementFilterItems(DomainMeta meta, CostEditorContext context);

        Task<IEnumerable<string>> GetInputLevelFilterItems(DomainMeta meta, CostEditorContext context);

        Task<IEnumerable<EditItem>> GetEditItems(DomainMeta meta, CostEditorContext context);

        Task<int> UpdateValues(IEnumerable<EditItem> editItems, DomainMeta meta, CostEditorContext context);
    }
}
