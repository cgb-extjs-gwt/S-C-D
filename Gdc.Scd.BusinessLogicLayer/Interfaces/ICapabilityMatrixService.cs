using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombination(CapabilityMatrixEditDto m);

        Task AllowCombinations(long[] items);

        Task DenyCombination(CapabilityMatrixEditDto m);

        IEnumerable<CapabilityMatrixDto> GetAllowedCombinations();

        IEnumerable<CapabilityMatrixDto> GetDeniedCombinations();
    }
}
