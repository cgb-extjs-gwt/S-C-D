using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Server.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

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
        public DataInfo<HwCostDto> GetHwCost(HwFilterDto filter, int start = 0, int limit = 50)
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            int total;
            IEnumerable<HwCostDto> items = calcSrv.GetHardwareCost(filter, start, limit, out total);

            return new DataInfo<HwCostDto> { Items = items, Total = total };
        }

        [HttpGet]
        public DataInfo<SwCostDto> GetSwCost(SwFilterDto filter, int start = 0, int limit = 50)
        {
            if (!isRangeValid(start, limit))
            {
                return null;
            }

            int total;
            IEnumerable<SwCostDto> items = calcSrv.GetSoftwareCost(filter, start, limit, out total);

            return new DataInfo<SwCostDto> { Items = items, Total = total };
        }

        [HttpPost]
        public void SaveHwCost([FromBody]IEnumerable<HwCostDto> records)
        {
            var model = records.Select(x => new HwCostManualDto
            {
                Id = x.Id,
                ServiceTC = x.ServiceTCManual,
                ServiceTP = x.ServiceTPManual
            });
            calcSrv.SaveHardwareCost(model);
        }

        private bool isRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }
    }
}
