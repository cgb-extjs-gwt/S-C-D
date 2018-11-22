using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockService
    {
        Task UpdateByCoordinatesAsync(IEnumerable<CostBlockEntityMeta> costBlockMetas);

        void UpdateByCoordinates(IEnumerable<CostBlockEntityMeta> costBlockMetas);

        Task UpdateByCoordinatesAsync();

        void UpdateByCoordinates();
    }
}
