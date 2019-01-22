using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockService
    {
        Task<QualityGateResultSet> Update(EditInfo[] editInfos, ApprovalOption approvalOption, EditorType editorType);

        Task<IEnumerable<NamedId>> GetCoordinateItems(HistoryContext context, string coordinateId);

        Task<IEnumerable<NamedId>> GetDependencyItems(HistoryContext context);

        Task<IEnumerable<NamedId>> GetRegions(HistoryContext context);

        Task<CostElementData> GetCostElementData(HistoryContext context);

        Task UpdateByCoordinatesAsync(
            IEnumerable<CostBlockEntityMeta> costBlockMetas, 
            IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(
            IEnumerable<CostBlockEntityMeta> costBlockMetas, 
            IEnumerable<UpdateQueryOption> updateOptions = null);

        Task UpdateByCoordinatesAsync(IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(IEnumerable<UpdateQueryOption> updateOptions = null);
    }
}
