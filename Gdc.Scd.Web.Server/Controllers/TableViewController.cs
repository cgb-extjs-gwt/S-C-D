using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Web.Server.Heplers;
using Gdc.Scd.Web.Server.Impl;

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
        public async Task UpdateRecords(IEnumerable<Record> records, [FromUri]ApprovalOption approvalOption)
        {
            await this.tableViewService.UpdateRecords(records, approvalOption);
        }

        [HttpGet]
        public async Task<TableViewInfo> GetTableViewInfo()
        {
            return await this.tableViewService.GetTableViewInfo();
        }

        // TODO: Need return DataInfo object, otherwise live scrol don't work. See BaseDomainController method GetBy.
        [HttpGet]
        public async Task<IEnumerable<HistoryItem>> GetHistory(
            [FromUri]CostElementIdentifier costElementId,
            [FromUri]IDictionary<string, long> coordinates,
            [FromUri]int? start,
            [FromUri]int? limit,
            [FromUri]string sort = null)
        {
            var queryInfo = QueryInfoHelper.BuildQueryInfo(start, limit, sort);

            return await this.tableViewService.GetHistoryItems(costElementId, coordinates, queryInfo);
        }
    }
}