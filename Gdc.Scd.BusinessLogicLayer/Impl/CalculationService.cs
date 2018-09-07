using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalculationService : ICalculationService
    {
        public IEnumerable<HwCostDto> GetHardwareCost(HwFilterDto filter, int start, int limit, out int count)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SwCostDto> GetSoftwareCost(SwFilterDto filter, int start, int limit, out int count)
        {
            throw new System.NotImplementedException();
        }

        public Task SaveHardwareResult()
        {
            throw new System.NotImplementedException();
        }

        public Task SaveSoftfwareResult()
        {
            throw new System.NotImplementedException();
        }
    }
}
