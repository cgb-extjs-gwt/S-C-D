using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.BusinessLogicLayer.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostEditorService
    {
        Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context);

        Task<IEnumerable<NamedId>> GetCostElementReferenceValues(CostEditorContext context);

        Task<CostElementData> GetCostElementData(CostEditorContext context);

        Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context);

        Task<QualityGateResultDto> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context, ApprovalOption approvalOption);
    }
}
