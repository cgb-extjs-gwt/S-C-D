using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IQualityGateSevice
    {
        Task<QualityGateResult> Check(EditContext editContext, EditorType editorType);

        Task<QualityGateResult> Check(CostBlockHistory history);

        QualityGateOption GetQualityGateOption(ICostElementIdentifier context, EditorType editorType);

        bool IsUseCheck(QualityGateOption option);
    }
}
