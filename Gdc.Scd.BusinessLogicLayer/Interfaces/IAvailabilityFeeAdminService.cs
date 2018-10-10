using Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee;
using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IAvailabilityFeeAdminService
    {
        List<AdminAvailabilityFeeDto> GetAllCombinations(int pageNumber, int limit, out int totalCount);

        void ApplyAvailabilityFeeForSelectedCombination(AdminAvailabilityFee model);

        void RemoveCombination(long id);

        void SaveCombinations(IEnumerable<AdminAvailabilityFeeViewDto> records);
    }
}
