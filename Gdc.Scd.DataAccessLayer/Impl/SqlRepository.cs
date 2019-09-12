using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
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

            return await this.repositorySet.ReadBySqlAsync(query, reader => reader[0].ToString());
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
            var filterInfos = new List<FilterInfo>();

            if (entityFilter != null)
            {
                filterInfos.Add(new FilterInfo
                {
                    Filter = entityFilter,
                    TableName = meta.Name
                });
            }

            if (referenceFilter != null)
            {
                var referenceField = (ReferenceFieldMeta)meta.GetField(referenceFieldName);

                filterInfos.Add(new FilterInfo
                {
                    Filter = referenceFilter,
                    TableName = referenceField.ReferenceMeta.Name
                });
            }

            return await this.GetDistinctItems(new DistinctItemsInfo
            {
                 Meta = meta,
                 ReferenceFieldName = referenceFieldName,
                 Filters = filterInfos
            });
        }

        public async Task<IEnumerable<NamedId>> GetDistinctItems(DistinctItemsInfo info)
        {
            if (info.Meta == null)
            {
                throw new ArgumentException($"'{nameof(info)}.{nameof(info.Meta)}' can not be null");
            }

            if (info.ReferenceFieldName == null)
            {
                throw new ArgumentException($"'{nameof(info)}.{nameof(info.ReferenceFieldName)}' can not be null");
            }

            var referenceField = (ReferenceFieldMeta)info.Meta.GetField(info.ReferenceFieldName);
            var conditions = new List<ConditionHelper>();

            if (info.Filters != null)
            {
                conditions.AddRange(
                    info.Filters.Where(filterInfo => filterInfo.Filter.Count > 0)
                                .Select(filterInfo => ConditionHelper.AndStatic(filterInfo.Filter, filterInfo.TableName)));
            }

            if (info.FilterCondition != null)
            {
                conditions.Add(info.FilterCondition);
            }

            var idColumn = new ColumnInfo(referenceField.ReferenceValueField, referenceField.ReferenceMeta.Name);
            var nameColumn = new ColumnInfo(referenceField.ReferenceFaceField, referenceField.ReferenceMeta.Name);

            var joinQuery =
                Sql.SelectDistinct(idColumn, nameColumn)
                   .From(info.Meta)
                   .Join(info.Meta, referenceField.Name);
            
            if (info.JoinInfos != null)
            {
                foreach (var joinInfo in info.JoinInfos)
                {
                    joinQuery = joinQuery.Join(joinInfo.JoinedMeta, joinInfo.JoinCondition, joinInfo.JoinType, joinInfo.JoinedAlias);
                }
            }

            var query =
                joinQuery.Where(conditions)
                     .OrderBy(SortDirection.Asc, nameColumn);

            return await this.repositorySet.ReadBySqlAsync(
                query,
                reader => new NamedId
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1)
                });
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

            return await this.repositorySet.ReadBySqlAsync(
                query,
                reader => new NamedId
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1)
                });
        }
    }
}
