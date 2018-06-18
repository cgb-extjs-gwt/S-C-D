using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class SqlRepository : ISqlRepository
    {
        private readonly IRepositorySet repositorySet;

        public SqlRepository(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public async Task<IEnumerable<string>> GetDistinctValues(
            string columnName, 
            string tableName, 
            string schemaName = null, 
            IDictionary<string, IEnumerable<object>> filter = null)
        {
            var query = Sql.SelectDistinct(columnName).From(tableName, schemaName).Where(filter);

            return await this.repositorySet.ReadFromDb(query, reader => reader.GetString(0));
        }

        public async Task<IEnumerable<string>> GetValues(
            string columnName,
            string tableName,
            string schemaName = null,
            IDictionary<string, IEnumerable<object>> filter = null)
        {
            var query = Sql.Select(columnName).From(tableName, schemaName).Where(filter);

            return await this.repositorySet.ReadFromDb(query, reader => reader.GetString(0));
        }
    }
}
