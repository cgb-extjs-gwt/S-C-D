using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockService
    {
        Task<QualityGateResultSet> Update(EditInfo[] editInfos, ApprovalOption approvalOption, EditorType editorType);

        Task<IEnumerable<NamedId>> GetCoordinateItems(CostElementContext context, string coordinateId);

        Task<IEnumerable<NamedId>> GetDependencyItems(CostElementContext context);

        Task<IEnumerable<NamedId>> GetRegions(CostElementContext context);

        Task<CostElementDataDto> GetCostElementData(CostElementContext context);

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
