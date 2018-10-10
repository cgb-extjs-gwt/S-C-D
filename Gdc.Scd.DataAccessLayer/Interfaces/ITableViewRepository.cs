using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ITableViewRepository
    {
        Task<IEnumerable<TableViewRecord>> GetRecords(TableViewCostElementInfo[] costBlockInfos);

        Task UpdateRecords(TableViewCostElementInfo[] tableViewInfos, IEnumerable<TableViewRecord> records);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(TableViewCostElementInfo[] costBlockInfos);

        TableViewRecordInfo GetTableViewRecordInfo(TableViewCostElementInfo[] costBlockInfos);
    }
}
