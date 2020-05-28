using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Web.Server.Entities;
using Gdc.Scd.Web.Server.Heplers;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.TableView })]
    public class TableViewController : ApiController
    {
        private readonly ITableViewService tableViewService;

        public TableViewController(ITableViewService tableViewService)
        {
            this.tableViewService = tableViewService;
        }

        [HttpGet]
        public async Task<IEnumerable<Record>> GetRecords()
        {
            return await this.tableViewService.GetRecords();
        }

        [HttpPost]
        public async Task<QualityGateResultSet> UpdateRecords(IEnumerable<Record> records, [FromUri]ApprovalOption approvalOption)
        {
            return await this.tableViewService.UpdateRecords(records, approvalOption);
        }

        [HttpGet]
        public async Task<TableViewInfo> GetTableViewInfo()
        {
            return await this.tableViewService.GetTableViewInfo();
        }

        [HttpGet]
        public async Task<DataInfo<HistoryItemDto>> GetHistory(
            [FromUri]CostElementIdentifier costElementId,
            [FromUri]string coordinates,
            [FromUri]int? start,
            [FromUri]int? limit,
            [FromUri]string sort = null)
        {
            var queryInfo = QueryInfoHelper.BuildQueryInfo(start, limit, sort);
            var coordinatesDict = JsonConvert.DeserializeObject<Dictionary<string, long>>(coordinates);
            return await this.tableViewService.GetHistory(costElementId, coordinatesDict, queryInfo);
        }

        public async Task<HttpResponseMessage> ExportToExcel()
        {
            try
            {
                var stream = await this.tableViewService.ExportToExcel();

                return this.ExcelContent(stream, "CentralDataInput.xlsx");
            }
            catch
            {
                return this.ExcelContent(new MemoryStream(), "Error");
            }
        }

        [HttpPost]
        public async Task<TableViewExcelImportResult> ImportExcel([FromBody]ImportData importData)
        {
            var bytes = Convert.FromBase64String(importData.ExcelFile);
            var stream = new MemoryStream(bytes);

            return await this.tableViewService.ImportFromExcel(stream, importData.ApprovalOption);
        }
    }
}