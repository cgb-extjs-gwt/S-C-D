using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICalculationService
    {
        Task<(string json, int total)> GetHardwareCost(bool approved, HwFilterDto filter, int start, int limit);

        Task<(string json, int total)> GetSoftwareCost(bool approved, SwFilterDto filter, int start, int limit);

        Task<(string json, int total)> GetSoftwareProactiveCost(bool approved, SwFilterDto filter, int start, int limit);

        void SaveHardwareCost(User changeUser, IEnumerable<HwCostManualDto> records, bool release = false);
    }
}
