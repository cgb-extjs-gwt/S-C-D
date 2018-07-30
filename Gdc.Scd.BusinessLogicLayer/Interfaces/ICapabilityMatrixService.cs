using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombination(CapabilityMatrixEditDto m);

        Task DenyCombination(CapabilityMatrixEditDto m);

        Task<IEnumerable<CapabilityMatrixAllow>> GetAllowedCombinations();

        Task<IEnumerable<CapabilityMatrixDeny>> GetDeniedCombinations();
    }
}
