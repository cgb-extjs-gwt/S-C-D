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
                [FromUri]HwFilterDto filter,
                [FromUri]bool approved = true,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (filter != null &&
                filter.Country > 0 &&
                IsRangeValid(start, limit) &&
                HasAccess(approved, filter.Country))
            {
                return calcSrv.GetHardwareCost(approved, filter, start, limit)
                              .ContinueWith(x => this.JsonContent(x.Result.json, x.Result.total));
            }
            throw this.NotFoundException();
        }

        [HttpGet]
        public Task<DataInfo<SwMaintenanceCostDto>> GetSwCost(
                [FromUri]SwFilterDto filter,
                [FromUri]bool approved = true,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (IsRangeValid(start, limit) &&
                HasAccess(approved))
            {
                return calcSrv.GetSoftwareCost(approved, filter, start, limit)
                              .ContinueWith(x => new DataInfo<SwMaintenanceCostDto> { Items = x.Result.items, Total = x.Result.total });
            }
            throw this.NotFoundException();
        }

        [HttpGet]
        public Task<DataInfo<SwProactiveCostDto>> GetSwProactiveCost(
               [FromUri]SwFilterDto filter,
               [FromUri]bool approved = true,
               [FromUri]int start = 0,
               [FromUri]int limit = 50
           )
        {
            if (filter != null &&
                IsRangeValid(start, limit) &&
                HasAccess(approved, filter.Country.GetValueOrDefault()))
            {
                return calcSrv.GetSoftwareProactiveCost(approved, filter, start, limit)
                              .ContinueWith(x => new DataInfo<SwProactiveCostDto> { Items = x.Result.items, Total = x.Result.total });
            }
            throw this.NotFoundException();
        }

        [HttpPost]
        public void SaveHwCost([FromBody]IEnumerable<HwCostDto> records)
        {
            if (HasAccess())
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
            throw this.NotFoundException();
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }

        private bool HasAccess()
        {
            return false;
        }

        private bool HasAccess(bool approved)
        {
            if (approved)
            {
                return true;
            }
            return approved;
        }

        private bool HasAccess(bool approved, long countryId)
        {
            if (approved)
            {
                return true;
            }
            return approved;
        }
    }
}
