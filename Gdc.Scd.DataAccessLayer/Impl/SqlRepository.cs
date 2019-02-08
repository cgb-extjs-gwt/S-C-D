using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
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

        public async Task<IEnumerable<NamedId>> GetDistinctItems(string entityName, string schema, string referenceFieldName, IDictionary<string, long[]> filter)
        {
            return await this.GetDistinctItems(entityName, schema, referenceFieldName, filter.Convert());
        }

        public async Task<IEnumerable<NamedId>> GetDistinctItems(
            string entityName,
            string schema,
            string referenceFieldName,
            IDictionary<string, IEnumerable<object>> entityFilter = null,
            IDictionary<string, IEnumerable<object>> referenceFilter = null,
            ConditionHelper filterCondition = null)
        {
            var meta = this.domainEnitiesMeta.GetEntityMeta(entityName, schema);

            return await this.GetDistinctItems(meta, referenceFieldName, entityFilter, referenceFilter);
        }

        public async Task<IEnumerable<NamedId>> GetDistinctItems(
            BaseEntityMeta meta, 
            string referenceFieldName, 
            IDictionary<string, IEnumerable<object>> entityFilter = null,
            IDictionary<string, IEnumerable<object>> referenceFilter = null,
            ConditionHelper filterCondition = null)
        {
            //var referenceField = (ReferenceFieldMeta)meta.GetField(referenceFieldName);
            //var conditions = 
            //    BuildCondition(entityFilter, meta.Name).Concat(BuildCondition(referenceFilter, referenceField.ReferenceMeta.Name)).ToList();

            //if (filterCondition != null)
            //{
            //    conditions.Add(filterCondition);
            //}

            //var idColumn = new ColumnInfo(referenceField.ReferenceValueField, referenceField.ReferenceMeta.Name);
            //var nameColumn = new ColumnInfo(referenceField.ReferenceFaceField, referenceField.ReferenceMeta.Name);

            //var query =
            //    Sql.SelectDistinct(idColumn, nameColumn)
            //       .From(meta)
            //       .Join(meta, referenceField.Name)
            //       .Join(joinInfos)
            //       .Where(conditions)
            //       .OrderBy(SortDirection.Asc, nameColumn);

            //return await this.repositorySet.ReadBySql(
            //    query,
            //    reader => new NamedId
            //    {
            //        Id = reader.GetInt64(0),
            //        Name = reader.GetString(1)
            //    });

            //IEnumerable<ConditionHelper> BuildCondition(IDictionary<string, IEnumerable<object>> filter, string tableName)
            //{
            //    if (filter != null && filter.Count > 0)
            //    {
            //        yield return ConditionHelper.AndStatic(filter, tableName, tableName);
            //    }
            //}

            var referenceField = (ReferenceFieldMeta)meta.GetField(referenceFieldName);
            var filters = new Dictionary<string, IDictionary<string, IEnumerable<object>>>
            {
                [meta.Name] = entityFilter,
                [referenceField.ReferenceMeta.Name] = referenceFilter
            };

            return await this.GetDistinctItems(meta, referenceFieldName, filters, filterCondition);
        }

        public async Task<IEnumerable<NamedId>> GetDistinctItems(
            BaseEntityMeta meta,
            string referenceFieldName,
            IDictionary<string, IDictionary<string, IEnumerable<object>>> filters,
            ConditionHelper filterCondition = null,
            IEnumerable<JoinInfo> joinInfos = null)
        {
            var referenceField = (ReferenceFieldMeta)meta.GetField(referenceFieldName);
            //var conditions =
            //    BuildCondition(entityFilter, meta.Name).Concat(BuildCondition(referenceFilter, referenceField.ReferenceMeta.Name)).ToList();
            var conditions = filters.SelectMany(filter => BuildCondition(filter.Value, filter.Key)).ToList();

            if (filterCondition != null)
            {
                conditions.Add(filterCondition);
            }

            var idColumn = new ColumnInfo(referenceField.ReferenceValueField, referenceField.ReferenceMeta.Name);
            var nameColumn = new ColumnInfo(referenceField.ReferenceFaceField, referenceField.ReferenceMeta.Name);

            var query =
                Sql.SelectDistinct(idColumn, nameColumn)
                   .From(meta)
                   .Join(meta, referenceField.Name)
                   .Join(joinInfos)
                   .Where(conditions)
                   .OrderBy(SortDirection.Asc, nameColumn);

            return await this.repositorySet.ReadBySql(
                query,
                reader => new NamedId
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1)
                });

            IEnumerable<ConditionHelper> BuildCondition(IDictionary<string, IEnumerable<object>> filter, string tableName)
            {
                if (filter != null && filter.Count > 0)
                {
                    yield return ConditionHelper.AndStatic(filter, tableName, tableName);
                }
            }
        }

        public async Task<IEnumerable<NamedId>> GetNameIdItems(BaseEntityMeta entityMeta, string idField, string nameField)
        {
            return await this.GetNameIdItems(entityMeta, idField, nameField, (IDictionary<string, IEnumerable<object>>)null);
        }

        public async Task<IEnumerable<NamedId>> GetNameIdItems(BaseEntityMeta entityMeta, string idField, string nameField, IEnumerable<long> ids)
        {
            Dictionary<string, IEnumerable<object>> filter = null;

            if (ids != null)
            {
                var idsArray = ids.Cast<object>().ToArray();
                if (idsArray.Length > 0)
                {
                    filter = new Dictionary<string, IEnumerable<object>>
                    {
                        [idField] = idsArray
                    };
                }
            }

            return await this.GetNameIdItems(entityMeta, idField, nameField, filter);
        }

        public async Task<IEnumerable<NamedId>> GetNameIdItems(BaseEntityMeta entityMeta, string idField, string nameField, IDictionary<string, IEnumerable<object>> filter)
        {
            SqlHelper query;

            var selectQuery = Sql.Select(idField, nameField).From(entityMeta);

            if (filter == null)
            {
                query = selectQuery;
            }
            else
            {

                if (filter.Count > 0)
                {
                    query = selectQuery.Where(filter);
                }
                else
                {
                    query = selectQuery;
                }
            }

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
