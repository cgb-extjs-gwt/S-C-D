using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class CostBlockController : ApiController
    {
        private readonly ICostImportExcelService costElementExcelService;

        private readonly ICostBlockService costBlockService;

        public CostBlockController(ICostImportExcelService costElementExcelService, ICostBlockService costBlockService)
        {
            this.costElementExcelService = costElementExcelService;
            this.costBlockService = costBlockService;
        }

        [HttpGet]
        public async Task<IEnumerable<NamedId>> GetRegions([FromUri]CostElementContext context)
        {
            return await this.costBlockService.GetRegions(context);
        }

        [HttpGet]
        public async Task<IEnumerable<NamedId>> GetDependencyItems([FromUri]CostEditorContext context)
        {
            return await this.costBlockService.GetDependencyItems(context);
        }

        [HttpPost]
        [ScdAuthorize(Permissions = new[] { PermissionConstants.CostImport })]
        public async Task<ExcelImportResult> ImportExcel([FromBody]ImportData importData)
        {
            var bytes = Convert.FromBase64String(importData.ExcelFile);
            var stream = new MemoryStream(bytes);

            return await this.costElementExcelService.Import(
                importData.CostElementId, 
                stream, 
                importData.ApprovalOption, 
                importData.DependencyItemId,
                importData.RegionId);
        }

        public class ImportData
        {
            public CostElementIdentifier CostElementId { get; set; }

            public ApprovalOption ApprovalOption { get; set; }

            public long? DependencyItemId { get; set; }

            public long? RegionId { get; set; }

            public string ExcelFile { get; set; }
        }
    }
}