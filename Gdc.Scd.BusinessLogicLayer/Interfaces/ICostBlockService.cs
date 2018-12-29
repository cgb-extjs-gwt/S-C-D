using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockService
    {
        Task UpdateByCoordinatesAsync(IEnumerable<CostBlockEntityMeta> costBlockMetas, 
            IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(IEnumerable<CostBlockEntityMeta> costBlockMetas, 
            IEnumerable<UpdateQueryOption> updateOptions = null);

        Task UpdateByCoordinatesAsync(IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(IEnumerable<UpdateQueryOption> updateOptions = null);
    }
}
