using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICapabilityMatrixService
    {
        Task AllowCombination();

        Task DenyCombination();

        Task<object> GetAllowedCombinations();

        Task<object> GetDenyedCombinations();
    }
}
