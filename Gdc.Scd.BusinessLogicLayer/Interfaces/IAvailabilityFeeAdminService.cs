using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Dto.AvailabilityFee;
using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
