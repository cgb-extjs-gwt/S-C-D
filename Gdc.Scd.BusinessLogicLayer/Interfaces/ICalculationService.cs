using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICalculationService
    {
        Task<Tuple<HwCostDto[], int>> GetHardwareCost(HwFilterDto filter, int start, int limit);

        Task<Tuple<SwCostDto[], int>> GetSoftwareCost(SwFilterDto filter, int start, int limit);

        void SaveHardwareCost(IEnumerable<HwCostManualDto> records);
    }
}
