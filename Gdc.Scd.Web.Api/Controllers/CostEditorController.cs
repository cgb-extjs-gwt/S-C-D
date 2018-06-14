using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Meta.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Scd.Web.Api.Controllers
{
    [Route("api/[controller]")]
    public class CostEditorController : Controller
    {
        private readonly ICostEditorService costEditorService;

        private readonly IDomainMetaSevice domainMetaSevice;

        private readonly ICountryService countryService;

        public CostEditorController(
            ICostEditorService costEditorService, 
            IDomainMetaSevice domainMetaSevice, 
            ICountryService countryService)
        {
            this.costEditorService = costEditorService;
            this.domainMetaSevice = domainMetaSevice;
            this.countryService = countryService;
        }

        [HttpGet]
        public async Task<CostEditorDto> GetCostEditorData()
        {
            return new CostEditorDto
            {
                Meta = this.domainMetaSevice.Get(),
                Countries = await this.countryService.GetAll()
            };
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetCostElementFilterItems(CostEditorContext context)
        {
            var meta = this.domainMetaSevice.Get();

            return await costEditorService.GetCostElementFilterItems(meta, context);
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetInputLevelFilterItems(CostEditorContext context)
        {
            return await costEditorService.GetInputLevelFilterItems(context);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            var meta = this.domainMetaSevice.Get();

            return await costEditorService.GetEditItems(meta, context);
        }
    }
}
