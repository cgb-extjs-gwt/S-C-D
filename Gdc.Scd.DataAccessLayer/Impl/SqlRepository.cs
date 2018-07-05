using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class SqlRepository : ISqlRepository
    {
        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public SqlRepository(IRepositorySet repositorySet, DomainEnitiesMeta domainEnitiesMeta)
        {
            this.repositorySet = repositorySet;
            this.domainEnitiesMeta = domainEnitiesMeta;
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

        public async Task<IEnumerable<NamedId>> GetDistinctItems(string entityName, string schema, string referenceFieldName, IDictionary<string, IEnumerable<object>> filter = null)
        {
            var meta = this.domainEnitiesMeta.GetEntityMeta(entityName, schema);
            var referenceField = (ReferenceFieldMeta)meta.GetField(referenceFieldName);

            var query =
                Sql.SelectDistinct(
                    new ColumnInfo(referenceField.ValueField, referenceField.ReferenceMeta.Name),
                    new ColumnInfo(referenceField.FaceValueField, referenceField.ReferenceMeta.Name))
                   .From(meta)
                   .Join(meta, referenceField.Name)
                   .Where(filter, meta.Name);

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
