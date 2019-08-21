using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IPivotGridRepository
    {
        Task<PivotResult> GetData(PivotRequest request, BaseEntityMeta meta);
    }
}
