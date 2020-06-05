using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICalculationService
    {
        Task<(string json, int total, bool hasMore)> GetHardwareCost(bool approved, HwFilterDto filter, int start, int limit);

        Task<(string json, int total, bool hasMore)> GetSoftwareCost(bool approved, SwFilterDto filter, int start, int limit);

        Task<(string json, int total, bool hasMore)> GetSoftwareProactiveCost(bool approved, SwFilterDto filter, int start, int limit);

        void SaveHardwareCost(User changeUser, IEnumerable<HwCostManualDto> records);

        void SaveStandardWarrantyCost(User user, HwCostDto[] items);

        Task ReleaseHardwareCost(User changeUser, HwFilterDto filter);

        Task ReleaseSelectedHardwareCost(User changeUser, HwFilterDto filter, HwCostDto[] items);
    }
}
