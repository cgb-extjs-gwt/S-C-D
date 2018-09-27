using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombinations(long[] items);

        Task DenyCombination(CapabilityMatrixRuleSetDto m);

        Task<Tuple<CapabilityMatrixDto[], int>> GetAllowedCombinations(int start, int limit);

        Task<Tuple<CapabilityMatrixDto[], int>> GetAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit);

        Task<Tuple<CapabilityMatrixDto[], int>> GetCountryAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit);

        Task<Tuple<CapabilityMatrixRuleDto[], int>> GetDeniedCombinations(CapabilityMatrixFilterDto filter, int start, int limit);
    }
}
