using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Newtonsoft.Json;

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
        public async Task<DataInfo<TableViewRecord>> GetRecords(
            [FromUri]int start,
            [FromUri]int limit,
            [FromUri]string sort = null, 
            [FromUri]Dictionary<ColumnInfo, IEnumerable<object>> filter = null)
        {
            var queryInfo = new QueryInfo
            {
                Skip = start,
                Take = limit
            };

            if (sort != null)
            {
                queryInfo.Sort = JsonConvert.DeserializeObject<SortInfo[]>(sort).FirstOrDefault();
            }

            return await this.tableViewService.GetRecords(queryInfo, filter);
        }

        [HttpPost]
        public async Task UpdateRecords(IEnumerable<TableViewRecord> records)
        {
            await this.tableViewService.UpdateRecords(records);
        }

        [HttpGet]
        public async Task<TableViewInfo> GetTableViewInfo()
        {
            return await this.tableViewService.GetTableViewInfo();
        }
    }
}