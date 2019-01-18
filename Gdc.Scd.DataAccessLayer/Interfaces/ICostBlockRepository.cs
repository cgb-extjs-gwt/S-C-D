using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockRepository
    {
        Task<int> Update(IEnumerable<EditInfo> editInfos);

        Task<int> UpdateByCoordinatesAsync(CostBlockEntityMeta meta, IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(CostBlockEntityMeta meta, IEnumerable<UpdateQueryOption> updateOptions = null);
    }
}
