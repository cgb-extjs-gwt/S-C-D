using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ITableViewRepository
    {
        Task<DataInfo<TableViewRecord>> GetRecords(TableViewCostElementInfo[] tableViewInfos, QueryInfo queryInfo, IDictionary<ColumnInfo, IEnumerable<object>> filter = null);

        Task UpdateRecords(TableViewCostElementInfo[] tableViewInfos, IEnumerable<TableViewRecord> records);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetFilters(TableViewCostElementInfo[] costBlockInfos);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(TableViewCostElementInfo[] costBlockInfos);

        TableViewRecordInfo GetTableViewRecordInfo(TableViewCostElementInfo[] costBlockInfos);
    }
}
