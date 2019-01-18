using System.IO;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostElementExcelService
    {
        Task<ExcelImportResult> Import(ICostElementIdentifier costElementId, Stream excelStream, long? dependencyItemId = null);
    }
}
