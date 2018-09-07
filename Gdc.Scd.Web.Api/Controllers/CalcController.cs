using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.Web.Api.Controllers
{
    [Produces("application/json")]
    public class CalcController : Controller
    {
        ICalculationService calcSrv;

        public CalcController(ICalculationService calculationService)
        {
            this.calcSrv = calculationService;
        }

        [HttpGet]
        public DataInfo<HwCostDto> GetHwCost(HwFilterDto filter, int start, int limit)
        {
            int total;
            IEnumerable<HwCostDto> items = calcSrv.GetHardwareCost(filter, start, limit, out total);

            return new DataInfo<HwCostDto> { Items = items, Total = total };
        }

        [HttpGet]
        public DataInfo<SwCostDto> GetSwCost(SwFilterDto filter, int start, int limit)
        {
            int total;
            IEnumerable<SwCostDto> items = calcSrv.GetSoftwareCost(filter, start, limit, out total);

            return new DataInfo<SwCostDto> { Items = items, Total = total };
        }

        [HttpPost]
        public Task SaveHwCost([FromBody]IEnumerable<HwCostManualDto> records)
        {
            return calcSrv.SaveHardwareCost(records);
        }

        [HttpPost]
        public Task SaveSwCost([FromBody]IEnumerable<SwCostManualDto> records)
        {
            return calcSrv.SaveSoftfwareCost(records);
        }
    }
}
