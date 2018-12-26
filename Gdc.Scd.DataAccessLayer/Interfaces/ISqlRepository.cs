using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ISqlRepository
    {
        Task<IEnumerable<string>> GetDistinctValues(string columnName,
            string tableName,
            string schemaName = null,
            IDictionary<string, IEnumerable<object>> filter = null);

        Task<IEnumerable<NamedId>> GetDistinctItems(string entityName, string schema, string referenceFieldName, IDictionary<string, long[]> filter);

        Task<IEnumerable<NamedId>> GetDistinctItems(
            string entityName, 
            string entitySchema, 
            string referenceFieldName, 
            IDictionary<string, IEnumerable<object>> entityFilter = null,
            IDictionary<string, IEnumerable<object>> referenceFilter = null);

        Task<IEnumerable<NamedId>> GetDistinctItems(
            BaseEntityMeta meta,
            string referenceFieldName,
            IDictionary<string, IEnumerable<object>> entityFilter = null,
            IDictionary<string, IEnumerable<object>> referenceFilter = null);

        Task<IEnumerable<NamedId>> GetNameIdItems(BaseEntityMeta entityMeta, string idField, string nameField, IEnumerable<long> ids = null);
    }
}
