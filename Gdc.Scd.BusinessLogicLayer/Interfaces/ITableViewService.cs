using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ITableViewService
    {
        Task<IEnumerable<TableViewRecord>> GetRecords(QueryInfo queryInfo, IDictionary<ColumnInfo, IEnumerable<object>> filter = null);

        Task UpdateRecords(IEnumerable<TableViewRecord> records);
    }
}
