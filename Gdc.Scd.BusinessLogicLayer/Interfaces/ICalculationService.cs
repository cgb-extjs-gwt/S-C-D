using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICalculationService
    {
        Task<(string json, int total)> GetHardwareCost(bool approved, HwFilterDto filter, int lasId, int limit);

        Task<(SwMaintenanceCostDto[] items, int total)> GetSoftwareCost(SwFilterDto filter, int start, int limit);

        Task<(SwProactiveCostDto[] items, int total)> GetSoftwareProactiveCost(SwFilterDto filter, int start, int limit);

        void SaveHardwareCost(IEnumerable<HwCostManualDto> records);
    }
}
