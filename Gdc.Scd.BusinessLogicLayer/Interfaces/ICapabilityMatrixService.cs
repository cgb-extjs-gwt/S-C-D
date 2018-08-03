using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombinations(long[] items);

        Task DenyCombination(CapabilityMatrixRuleSetDto m);

        IEnumerable<CapabilityMatrixDto> GetAllowedCombinations();

        IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(CapabilityMatrixFilterDto filter);

        IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations();

        IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations(CapabilityMatrixFilterDto filter);
    }
}
