using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.Web.Server.Heplers;
using Gdc.Scd.Web.Server.Impl;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gdc.Scd.Web.Server.Controllers
{

    [ScdAuthorize(Permissions = new[] { PermissionConstants.CostEditor })]
    public class CostEditorController : System.Web.Http.ApiController
    {
        private readonly ICostEditorService costEditorService;

        private readonly IDomainMetaSevice domainMetaSevice;

        public CostEditorController(
            ICostEditorService costEditorService, 
            IDomainMetaSevice domainMetaSevice,
            DomainMeta meta)
        {
            this.costEditorService = costEditorService;
            this.domainMetaSevice = domainMetaSevice;
        }

        public async Task<CostEditorDto> GetCostElementData([System.Web.Http.FromUri]CostEditorContext context)
        {
            return await this.costEditorService.GetCostElementData(context);
        }

        [HttpGet]
        public async Task<IEnumerable<NamedId>> GetInputLevelFilterItems([System.Web.Http.FromUri]CostEditorContext context)
        {
            return await this.costEditorService.GetInputLevelFilterItems(context);
        }

        [HttpGet]
        public async Task<IEnumerable<EditItem>> GetEditItems([System.Web.Http.FromUri]CostEditorContext context)
        {
            return await this.costEditorService.GetEditItems(context);
        }

        [HttpPost]
        public async Task<QualityGateResult> UpdateValues(
            [System.Web.Http.FromBody]IEnumerable<EditItem> editItems, 
            [System.Web.Http.FromUri]CostEditorContext context, 
            [System.Web.Http.FromUri]ApprovalOption approvalOption)
        {
            return await this.costEditorService.UpdateValues(editItems, context, approvalOption);
        }

        [HttpGet]
        public async Task<DataInfo<HistoryItemDto>> GetHistory(
            [System.Web.Http.FromUri]CostEditorContext context,
            [System.Web.Http.FromUri]long editItemId,
            [System.Web.Http.FromUri]int? start,
            [System.Web.Http.FromUri]int? limit,
            [System.Web.Http.FromUri]string sort = null)
        {
            var queryInfo = QueryInfoHelper.BuildQueryInfo(start, limit, sort);
            return await this.costEditorService.GetHistory(context, editItemId, queryInfo);
        }
    }
}
