using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockRepository
    {
        Task<int> UpdateByCoordinatesAsync(CostBlockEntityMeta meta, 
            IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(CostBlockEntityMeta meta, 
            IEnumerable<UpdateQueryOption> updateOptions = null);
    }
}
