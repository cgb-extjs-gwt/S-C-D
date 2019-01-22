using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostEditorService
    {
        Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context);

        Task<IEnumerable<NamedId>> GetCostElementReferenceValues(CostEditorContext context);

        Task<CostEditorCostElementData> GetCostElementData(CostEditorContext context);

        Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context);

        Task<QualityGateResult> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context, ApprovalOption approvalOption);

        Task<IEnumerable<HistoryItem>> GetHistoryItems(CostEditorContext context, long editItemId, QueryInfo queryInfo = null);
    }
}
