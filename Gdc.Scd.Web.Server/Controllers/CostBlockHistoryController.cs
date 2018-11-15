using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CostBlockHistoryController : ApiController
    {
        private readonly ICostBlockHistoryService costBlockHistoryService;

        public CostBlockHistoryController(ICostBlockHistoryService costBlockHistoryService)
        {
            this.costBlockHistoryService = costBlockHistoryService;
        }

        // TODO: Need return DataInfo object, otherwise live scrol don't work. See BaseDomainController method GetBy.
        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.CostEditor })]
        public async Task<IEnumerable<HistoryItem>> GetCostEditorHistory(
            [FromUri]CostEditorContext context,
            [FromUri]long editItemId,
            [FromUri]int? start,
            [FromUri]int? limit,
            [FromUri]string sort = null)
        {
            var queryInfo = this.GetQueryInfo(start, limit, sort);

            return await this.costBlockHistoryService.GetHistoryItems(context, editItemId, queryInfo);
        }

        // TODO: Need return DataInfo object, otherwise live scrol don't work. See BaseDomainController method GetBy.
        [HttpGet]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.TableView })]
        public async Task<IEnumerable<HistoryItem>> GetTableViewHistory(
            [FromUri]CostElementIdentifier costElementId,
            [FromUri]IDictionary<string, long> coordinates,
            [FromUri]int? start,
            [FromUri]int? limit,
            [FromUri]string sort = null)
        {
            var queryInfo = this.GetQueryInfo(start, limit, sort);

            return await this.costBlockHistoryService.GetHistoryItems(costElementId, coordinates, queryInfo);
        }

        private QueryInfo GetQueryInfo(int? start, int? limit, string sort)
        {
            QueryInfo queryInfo = null;

            if (start.HasValue || limit.HasValue || sort != null)
            {
                queryInfo = new QueryInfo
                {
                    Skip = start,
                    Take = limit
                };

                if (sort != null)
                {
                    queryInfo.Sort = JsonConvert.DeserializeObject<SortInfo[]>(sort).FirstOrDefault();
                }
            }

            return queryInfo;
        }
    }
}
