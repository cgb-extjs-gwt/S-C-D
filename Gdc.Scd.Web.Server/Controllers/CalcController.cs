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

        [HttpPost]
        public Task<HttpResponseMessage> GetHwCost(
                [FromBody]HwFilterDto filter
            )
        {
            if (filter != null &&
                filter.Country != null &&
                filter.Country.Length > 0 &&
                IsRangeValid(filter.Start, filter.Limit) &&
                HasAccess(filter.Approved, filter.Country))
            {
                return calcSrv.GetHardwareCost(filter.Approved, filter, filter.Start, filter.Limit)
                              .ContinueWith(x =>
                              {
                                  var total = (filter.Page - 1) * filter.Limit + x.Result.total;
                                  if (x.Result.hasMore)
                                  {
                                      total++;
                                  }
                                  return this.JsonContent(x.Result.json, total);
                              });
            }
            else
            {
                return this.NotFoundContentAsync();
            }
        }

        [HttpPost]
        public Task<HttpResponseMessage> GetSwCost(
                [FromBody]SwFilterDto filter
            )
        {
            if (IsRangeValid(filter.Start, filter.Limit))
            {
                return calcSrv.GetSoftwareCost(filter.Approved, filter, filter.Start, filter.Limit)
                              .ContinueWith(x =>
                              {
                                  var total = (filter.Page - 1) * filter.Limit + x.Result.total;
                                  if (x.Result.hasMore)
                                  {
                                      total++;
                                  }
                                  return this.JsonContent(x.Result.json, total);
                              });
            }
            else
            {
                return this.NotFoundContentAsync();
            }
        }

        [HttpPost]
        public Task<HttpResponseMessage> GetSwProactiveCost([FromBody]SwFilterDto filter)
        {
            if (filter != null &&
                filter.Country != null &&
                filter.Country.Length > 0 &&
                IsRangeValid(filter.Start, filter.Limit) &&
                HasAccess(filter.Approved, filter.Country))
            {
                return calcSrv.GetSoftwareProactiveCost(filter.Approved, filter, filter.Start, filter.Limit)
                              .ContinueWith(x =>
                              {
                                  var total = (filter.Page - 1) * filter.Limit + x.Result.total;
                                  if (x.Result.hasMore)
                                  {
                                      total++;
                                  }
                                  return this.JsonContent(x.Result.json, total);
                              });
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
                    ReActiveTP = x.ReActiveTPManual,
                    ListPrice = x.ListPrice,
                    DealerDiscount = x.DealerDiscount
                });

                var usr = this.CurrentUser();

                calcSrv.SaveHardwareCost(usr, items);
                calcSrv.SaveStandardWarrantyCost(usr, m.Items);
            }
            else
            {
                throw this.NotFoundException();
            }
        }

        [HttpPost]
        public Task ReleaseHwCost([FromBody]SaveCostManualDto m)
        {
            if (HasAccess(m.CountryId))
            {
                return calcSrv.ReleaseSelectedHardwareCost(this.CurrentUser(), m.Filter, m.Items);
            }

            throw this.NotFoundException();
        }

        [HttpPost]
        public Task ReleaseHwCostAll([FromBody]HwFilterDto filter)
        {
            if (filter?.Country != null
                && filter.Country.Length > 0
                && HasAccess(false, filter.Country))
            {
                return calcSrv.ReleaseHardwareCost(this.CurrentUser(), filter);
            }

            return this.NotFoundContentAsync();
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 100;
        }

        private bool HasAccess(long[] countryIds)
        {
            var hasAccess = true;
            for (var i = 0; i < countryIds.Length; i++)
            {
                hasAccess = hasAccess && userCountrySrv.HasCountryAccess(this.CurrentUser(), countryIds[i]);
            }
            return hasAccess;
        }

        private bool HasAccess(long countryId)
        {
            return userCountrySrv.HasCountryAccess(this.CurrentUser(), countryId);
        }

        private bool HasAccess(bool approved, long[] countryIds)
        {
            if (approved)
            {
                return true;
            }

            var hasAccess = true;
            for (var i = 0; i < countryIds.Length; i++)
            {
                hasAccess = hasAccess && userCountrySrv.HasCountryAccess(this.CurrentUser(), countryIds[i]);
            }
            return hasAccess;
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

        public HwFilterDto Filter { get; set; }

        public HwCostDto[] Items { get; set; }
    }
}
