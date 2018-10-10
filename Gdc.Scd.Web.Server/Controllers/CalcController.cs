using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Controllers
{
    public class CalcController : ApiController
    {
        ICalculationService calcSrv;

        public CalcController(ICalculationService calculationService)
        {
            this.calcSrv = calculationService;
        }

        [HttpGet]
        public Task<DataInfo<HwCostDto>> GetHwCost(
                [FromUri]HwFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            return calcSrv.GetHardwareCost(filter, start, limit)
                          .ContinueWith(x => new DataInfo<HwCostDto>
                          {
                              Items = x.Result.Item1,
                              Total = x.Result.Item2
                          });
        }

        [HttpGet]
        public Task<DataInfo<SwCostDto>> GetSwCost(
                [FromUri]SwFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            return calcSrv.GetSoftwareCost(filter, start, limit)
                          .ContinueWith(x => new DataInfo<SwCostDto>
                          {
                              Items = x.Result.Item1,
                              Total = x.Result.Item2
                          });
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

        private bool isRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }
}
