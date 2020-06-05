using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockRepository
    {
        Task<int> Update(IEnumerable<EditInfo> editInfos);

        Task<int> UpdateByCoordinatesAsync(CostBlockEntityMeta meta, IEnumerable<UpdateQueryOption> updateOptions = null);

        void UpdateByCoordinates(CostBlockEntityMeta meta, IEnumerable<UpdateQueryOption> updateOptions = null);

        void CreatRegionIndexes();

        SqlHelper BuildUpdateByCoordinatesQuery(CostBlockEntityMeta meta, IEnumerable<UpdateQueryOption> updateOptions = null);

        Task<NamedId[]> GetDependencyByPortfolio(CostElementContext context);
    }
}
