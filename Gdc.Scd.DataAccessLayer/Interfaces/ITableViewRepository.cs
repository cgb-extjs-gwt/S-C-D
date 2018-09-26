using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ITableViewRepository
    {
        Task<IEnumerable<TableViewRecord>> GetRecords(TableViewQueryInfo[] tableViewInfos, QueryInfo queryInfo, IDictionary<ColumnInfo, IEnumerable<object>> filter = null);

        Task UpdateRecords(TableViewQueryInfo[] tableViewInfos, IEnumerable<TableViewRecord> records);
    }
}
