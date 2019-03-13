using System.IO;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostImportExcelService
    {
        Task<ExcelImportResult> Import(CostImportContext context, Stream excelStream, ApprovalOption approvalOption);
    }
}
