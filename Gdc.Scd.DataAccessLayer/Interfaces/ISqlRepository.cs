using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ISqlRepository
    {
        Task<IEnumerable<string>> GetDistinctValues(string columnName,
            string tableName,
            string schemaName = null,
            IDictionary<string, IEnumerable<object>> filter = null);

        Task<IEnumerable<string>> GetValues(
            string columnName,
            string tableName,
            string schemaName = null,
            IDictionary<string, IEnumerable<object>> filter = null);
    }
}
