using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IHddRetentionService
    {
        Task<(HddRetentionDto[] items, int total)> GetCost(bool approved, object filter, int start, int limit);
    }
}
