using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ISqlRepository
    {
        Task<IEnumerable<string>> GetDistinctValues(string columnName,
            string tableName,
            string schemaName = null,
            IDictionary<string, IEnumerable<object>> filter = null);

        Task<IEnumerable<NamedId>> GetDistinctItems(string entityName, string entityNamespace, string referenceFieldName, IDictionary<string, IEnumerable<object>> filter = null);
    }
}
