using Gdc.Scd.BusinessLogicLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombination();

        Task DenyCombination();

        Task<IEnumerable<CapabilityMatrixAllow>> GetAllowedCombinations();

        Task<IEnumerable<CapabilityMatrixDeny>> GetDeniedCombinations();
    }
}
