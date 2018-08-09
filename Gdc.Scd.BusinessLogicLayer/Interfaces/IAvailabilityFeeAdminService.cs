using Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IAvailabilityFeeAdminService
    {
        Task<List<AdminAvailabilityFeeDto>> GetAllCombinations();

        void ApplyAvailabilityFeeForSelectedCombination(AdminAvailabilityFeeDto model);

        void RemoveCombination(long id);
    }
}
