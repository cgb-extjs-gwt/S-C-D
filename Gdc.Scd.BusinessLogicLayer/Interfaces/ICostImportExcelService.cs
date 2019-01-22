using System.IO;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostImportExcelService
    {
        Task<ExcelImportResult> Import(
            ICostElementIdentifier costElementId, 
            Stream excelStream, 
            ApprovalOption approvalOption, 
            long? dependencyItemId = null,
            long? regionId = null);
    }
}
