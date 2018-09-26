using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class TableViewController : ApiController
    {
        private readonly ITableViewService tableViewService;

        public TableViewController(ITableViewService tableViewService)
        {
            this.tableViewService = tableViewService;
        }

        [HttpGet]
        public async Task<IEnumerable<TableViewRecord>> GetRecords([FromUri]QueryInfo queryInfo, [FromUri]Dictionary<ColumnInfo, IEnumerable<object>> filter = null)
        {
            return await this.tableViewService.GetRecords(queryInfo, filter);
        }

        [HttpPost]
        public async Task UpdateRecords(IEnumerable<TableViewRecord> records)
        {
            await this.tableViewService.UpdateRecords(records);
        }

        [HttpGet]
        public async Task<TableViewInfoDto> GetTableViewInfo()
        {
            return await this.tableViewService.GetTableViewInfo();
        }
    }
}