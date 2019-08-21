using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Pivot;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPortfolioPivotGridService
    {
        Task<PivotResult> GetData(PivotRequest request);
    }
}
