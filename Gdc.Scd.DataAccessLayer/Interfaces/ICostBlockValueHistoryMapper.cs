using System.Data;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockValueHistoryMapper
    {
        CostBlockValueHistory Map(CostBlockEntityMeta costBlockMeta, IDataReader reader);

        CostBlockValueHistory MapWithHistoryId(CostBlockEntityMeta costBlockMeta, IDataReader reader);

        CostBlockValueHistory MapWithQualityGate(CostBlockEntityMeta costBlockMeta, IDataReader reader);
    }
}
