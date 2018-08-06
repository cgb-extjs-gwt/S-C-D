using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombinations(long[] items);

        Task DenyCombination(CapabilityMatrixRuleSetDto m);

        IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(int start, int limit, out int count);

        IEnumerable<CapabilityMatrixDto> GetAllowedCombinations(CapabilityMatrixFilterDto filter, int start, int limit, out int count);

        IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations(int start, int limit, out int count);

        IEnumerable<CapabilityMatrixRuleDto> GetDeniedCombinations(CapabilityMatrixFilterDto filter, int start, int limit, out int count);
    }
}
