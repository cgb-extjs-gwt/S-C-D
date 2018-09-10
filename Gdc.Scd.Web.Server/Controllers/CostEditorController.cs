using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;


namespace Gdc.Scd.Web.Server.Controllers
{

    public class CostEditorController : System.Web.Http.ApiController
    {
        private readonly ICostEditorService costEditorService;

        private readonly IDomainMetaSevice domainMetaSevice;

        private readonly IDomainService<Country> countryService;

        private readonly DomainMeta meta;

        public CostEditorController(
            ICostEditorService costEditorService, 
            IDomainMetaSevice domainMetaSevice,
            IDomainService<Country> countryService,
            DomainMeta meta)
        {
            this.costEditorService = costEditorService;
            this.domainMetaSevice = domainMetaSevice;
            this.countryService = countryService;
            this.meta = meta;
        }

        [HttpGet]
        public DomainMeta GetCostEditorData()
        {
            return this.meta;
        }

        public async Task<CostElementData> GetCostElementData([System.Web.Http.FromUri]CostEditorContext context)
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
        public async Task<ActionResult> UpdateValues([System.Web.Http.FromBody]IEnumerable<EditItem> editItems, [System.Web.Http.FromUri]CostEditorContext context, bool forApproval)
        {
            await this.costEditorService.UpdateValues(editItems, context, forApproval);

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }
    }
}
