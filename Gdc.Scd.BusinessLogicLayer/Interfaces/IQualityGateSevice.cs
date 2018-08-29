using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IQualityGateSevice
    {
        Task<QualityGateResult> Check(IEnumerable<EditItem> editItems, CostEditorContext context);
    }
}
