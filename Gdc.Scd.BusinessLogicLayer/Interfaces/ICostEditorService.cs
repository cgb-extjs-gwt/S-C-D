using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostEditorService
    {
        Task<IEnumerable<NamedId>> GetCostElementFilterItems(CostEditorContext context);

        Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context);

        Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context);

        Task<int> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context);
    }
}
