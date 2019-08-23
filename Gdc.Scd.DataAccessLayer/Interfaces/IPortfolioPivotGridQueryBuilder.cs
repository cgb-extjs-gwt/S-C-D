using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IPortfolioPivotGridQueryBuilder
    {
        EntityMetaQuery Build(PortfolioPivotRequest request);
    }
}
