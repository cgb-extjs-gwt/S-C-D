using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
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

            return await this.repositorySet.ReadBySql(query, reader => reader[0].ToString());
        }

        public async Task<IEnumerable<NamedId>> GetDistinctItems(
            ColumnInfo idColumn,
            ColumnInfo nameColumn,
            string schemaName = null,
            IDictionary<string, IEnumerable<object>> filter = null)
        {
            var query =
                Sql.SelectDistinct(idColumn, nameColumn)
                   .From(idColumn.TableName, schemaName)
                   .Join(schemaName, nameColumn.TableName, SqlOperators.Equals(idColumn, nameColumn));

            return await this.repositorySet.ReadBySql(
                query, 
                reader => new NamedId
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1)
                });
        }
    }
}
