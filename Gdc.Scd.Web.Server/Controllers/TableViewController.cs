using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities.TableView;
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
        public async Task UpdateRecords(IEnumerable<Record> records, [FromUri]bool isApproving)
        {
            await this.tableViewService.UpdateRecords(records, isApproving);
        }

        [HttpGet]
        public async Task<TableViewInfo> GetTableViewInfo()
        {
            return await this.tableViewService.GetTableViewInfo();
        }
    }
}