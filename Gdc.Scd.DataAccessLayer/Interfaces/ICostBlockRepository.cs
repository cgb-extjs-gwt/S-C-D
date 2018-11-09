using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockRepository
    {
        Task<int> UpdateByCoordinatesAsync(CostBlockEntityMeta meta);

        void UpdateByCoordinates(CostBlockEntityMeta meta);
    }
}
