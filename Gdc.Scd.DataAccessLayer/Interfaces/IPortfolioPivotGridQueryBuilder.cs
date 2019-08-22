using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IPortfolioPivotGridQueryBuilder
    {
        EntityMetaQuery Build(PivotRequest request);
    }
}
