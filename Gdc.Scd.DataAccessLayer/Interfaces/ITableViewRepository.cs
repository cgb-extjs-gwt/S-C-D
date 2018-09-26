using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ITableViewRepository
    {
        Task<IEnumerable<TableViewRecord>> GetRecords(TableViewCostBlockInfo[] tableViewInfos, QueryInfo queryInfo, IDictionary<ColumnInfo, IEnumerable<object>> filter = null);

        Task UpdateRecords(TableViewCostBlockInfo[] tableViewInfos, IEnumerable<TableViewRecord> records);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetFilters(TableViewCostBlockInfo[] costBlockInfos);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(TableViewCostBlockInfo[] costBlockInfos);
    }
}
