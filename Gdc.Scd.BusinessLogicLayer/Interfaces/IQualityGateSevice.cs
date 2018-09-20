using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Web.BusinessLogicLayer.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IQualityGateSevice
    {
        Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, CostEditorContext context);

        Task<QualityGateResult> Check(CostBlockHistory history);

        Task<QualityGateResultDto> CheckAsQualityGateResultDto(CostBlockHistory history);

        Task<QualityGateResultDto> CheckAsQualityGateResultDto(IEnumerable<EditItem> editItems, CostEditorContext context);

        bool IsUseCheck(ICostElementIdentifier context);
    }
}
