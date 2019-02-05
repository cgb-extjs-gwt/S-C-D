using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Web.Server;
using Gdc.Scd.Web.Server.Impl;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Api.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Report })]
    public class CalcController : ApiController
    {
        private readonly ICalculationService calcSrv;

        private readonly ICountryUserService userCountrySrv;

        public CalcController(
                ICalculationService calcSrv,
                ICountryUserService userCountrySrv
            )
        {
            this.calcSrv = calcSrv;
            this.userCountrySrv = userCountrySrv;
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
            else
            {
                return this.NotFoundContentAsync();
            }
        }

        [HttpGet]
        public Task<HttpResponseMessage> GetSwCost(
                [FromUri]SwFilterDto filter,
                [FromUri]bool approved = true,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (IsRangeValid(start, limit))
            {
                return calcSrv.GetSoftwareCost(approved, filter, start, limit)
                              .ContinueWith(x => this.JsonContent(x.Result.json, x.Result.total));
            }
            else
            {
                return this.NotFoundContentAsync();
            }
        }

        [HttpGet]
        public Task<HttpResponseMessage> GetSwProactiveCost(
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
                              .ContinueWith(x => this.JsonContent(x.Result.json, x.Result.total));
            }
            else
            {
                return this.NotFoundContentAsync();
            }
        }

        [HttpPost]
        public void SaveHwCost([FromBody]SaveCostManualDto m)
        {
            if (HasAccess(m.CountryId))
            {
                var items = m.Items.Select(x => new HwCostManualDto
                {
                    Id = x.Id,
                    ServiceTC = x.ServiceTCManual,
                    ServiceTP = x.ServiceTPManual,
                    ListPrice = x.ListPrice,
                    DealerDiscount = x.DealerDiscount
                });
                calcSrv.SaveHardwareCost(this.CurrentUser(), m.CountryId, items);
            }
            else
            {
                throw this.NotFoundException();
            }
        }

        [HttpPost]
        public void ReleaseHwCost([FromBody]SaveCostManualDto m)
        {
            if (HasAccess(m.CountryId))
            {
                var items = m.Items.Select(x => new HwCostManualDto
                {
                    Id = x.Id,
                    ServiceTP_Released = x.ServiceTPManual ?? x.ServiceTP
                });
                calcSrv.SaveHardwareCost(this.CurrentUser(), m.CountryId, items, true);
            }
            else
            {
                throw this.NotFoundException();
            }
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }

        private bool HasAccess(long countryId)
        {
            return userCountrySrv.HasCountryAccess(this.CurrentUser(), countryId);
        }

        private bool HasAccess(bool approved, long countryId)
        {
            if (approved)
            {
                return true;
            }

            return userCountrySrv.HasCountryAccess(this.CurrentUser(), countryId);
        }
    }

    public class SaveCostManualDto
    {
        public long CountryId { get; set; }

        public HwCostDto[] Items { get; set; }
    }
}
