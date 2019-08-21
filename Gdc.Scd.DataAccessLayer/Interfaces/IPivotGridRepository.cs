using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IPivotGridRepository
    {
        Task<PivotResult> GetData(PivotRequest request, BaseEntityMeta meta, SqlHelper customQuery = null);
    }
}
