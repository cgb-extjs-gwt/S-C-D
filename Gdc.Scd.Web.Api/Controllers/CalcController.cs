using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Web.Api.Entities;
using Microsoft.AspNetCore.Mvc;
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
        public DataInfo<HwCostDto> Hardware(HwFilterDto filter, int start, int limit)
        {
            int total;
            var items = calcSrv.GetHardwareCost(filter, start, limit, out total);

            return new DataInfo<HwCostDto> { Items = items, Total = total };
        }

        [HttpGet]
        public DataInfo<SwCostDto> Software(SwFilterDto filter, int start, int limit)
        {
            int total;
            var items = calcSrv.GetSoftwareCost(filter, start, limit, out total);

            return new DataInfo<SwCostDto> { Items = items, Total = total };
        }

        [HttpPost]
        public Task Hardware()
        {
            return calcSrv.SaveHardwareResult();
        }

        [HttpPost]
        public Task Software()
        {
            return calcSrv.SaveSoftfwareResult();
        }
    }
}
