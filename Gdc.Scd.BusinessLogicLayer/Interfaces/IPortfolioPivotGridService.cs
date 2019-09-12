using System.IO;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Entities.Portfolio;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPortfolioPivotGridService
    {
        Task<PivotResult> GetData(PortfolioPivotRequest request);

        Task<Stream> PivotExcelExport(PortfolioPivotRequest request);
    }
}
