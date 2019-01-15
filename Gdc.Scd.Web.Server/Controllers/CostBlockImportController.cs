using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CostBlockImportController : ApiController
    {
        private readonly ICostElementExcelService costElementExcelService;

        public CostBlockImportController(ICostElementExcelService costElementExcelService)
        {
            this.costElementExcelService = costElementExcelService;
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ExcelImportResult> ImportExcel(ICostElementIdentifier costElementId, HttpPostedFileBase excelFile)
        {
            return await this.costElementExcelService.Import(costElementId, excelFile.InputStream);
        }
    }
}