using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICalculationService
    {
        IEnumerable<HwCostDto> GetHardwareCost(HwFilterDto filter, int start, int limit, out int count);

        IEnumerable<SwCostDto> GetSoftwareCost(SwFilterDto filter, int start, int limit, out int count);

        Task SaveHardwareCost(IEnumerable<HwCostManualDto> records);

        Task SaveSoftfwareCost(IEnumerable<SwCostManualDto> records);
    }
}
