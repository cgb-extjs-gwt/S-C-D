using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICalculationService
    {
        Task SaveHardwareResult();

        Task SaveSoftfwareResult();

        IEnumerable<HwCostDto> GetHardwareCost(HwFilterDto filter, int start, int limit, out int count);

        IEnumerable<SwCostDto> GetSoftwareCost(SwFilterDto filter, int start, int limit, out int count);
    }
}
