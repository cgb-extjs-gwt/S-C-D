using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server;
using Gdc.Scd.Web.Server.Impl;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Api.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Report })]
    public class CalcController : ApiController
    {
        ICalculationService calcSrv;

        public CalcController(ICalculationService calculationService)
        {
            this.calcSrv = calculationService;
        }

        [HttpGet]
        public Task<HttpResponseMessage> GetHwCost(
                [FromUri]bool approved,
                [FromUri]HwFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }

            return calcSrv.GetHardwareCost(approved, filter, start, limit)
                          .ContinueWith(x => this.JsonContent(x.Result.Json, x.Result.Total));
        }

        [HttpGet]
        public Task<DataInfo<SwMaintenanceCostDto>> GetSwCost(
                [FromUri]SwFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }

            return calcSrv.GetSoftwareCost(filter, start, limit)
                          .ContinueWith(x => new DataInfo<SwMaintenanceCostDto> { Items = x.Result.Item1, Total = x.Result.Item2 });
        }

        [HttpGet]
        public Task<DataInfo<SwProactiveCostDto>> GetSwProactiveCost(
               [FromUri]SwFilterDto filter,
               [FromUri]int start = 0,
               [FromUri]int limit = 50
           )
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }

            return calcSrv.GetSoftwareProactiveCost(filter, start, limit)
                          .ContinueWith(x => new DataInfo<SwProactiveCostDto> { Items = x.Result.Item1, Total = x.Result.Item2 });
        }

        [HttpPost]
        public void SaveHwCost([FromBody]IEnumerable<HwCostDto> records)
        {
            var model = records.Select(x => new HwCostManualDto
            {
                Id = x.Id,
                ServiceTC = x.ServiceTCManual,
                ServiceTP = x.ServiceTPManual,
                ListPrice = x.ListPrice,
                DealerDiscount = x.DealerDiscount
            });
            calcSrv.SaveHardwareCost(model);
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }
}
