using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IQualityGateSevice
    {
        Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, HistoryContext context, IDictionary<string, long[]> filter, EditorType editorType);

        Task<QualityGateResult> Check(CostBlockHistory history);

        QualityGateOption GetQualityGateOption(ICostElementIdentifier context, EditorType editorType);

        bool IsUseCheck(QualityGateOption option);
    }
}
