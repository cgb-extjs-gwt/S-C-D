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
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class TableViewRepository : ITableViewRepository
    {
        private const char AliasSeparator = '.';

        private readonly IRepositorySet repositorySet;

        private readonly ISqlRepository sqlRepository;

        public TableViewRepository(IRepositorySet repositorySet, ISqlRepository sqlRepository)
        {
            this.repositorySet = repositorySet;
            this.sqlRepository = sqlRepository;
        }

        public async Task<IEnumerable<TableViewRecord>> GetRecords(TableViewCostElementInfo[] costBlockInfos)
        {
            var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
            var columnInfo = this.BuildTableViewColumnInfo(costBlockInfos, coordinateFieldInfos);
            var recordsQuery = this.BuildGetRecordsQuery(costBlockInfos, columnInfo, coordinateFieldInfos);

            return await this.repositorySet.ReadBySql(recordsQuery, reader =>
            {
                var record = new TableViewRecord();

                foreach (var coordinate in columnInfo.CoordinateInfos)
                {
                    record.Coordinates.Add(
                        coordinate.Id.Alias,
                        new NamedId
                        {
                            Id = (long)reader[coordinate.Id.Alias],
                            Name = (string)reader[coordinate.Name.Alias]
                        });
                }

                foreach (var data in columnInfo.DataInfos)
                {
                    record.Data.Add(
                        data.Value.Alias,
                        new ValueCount
                        {
                            Value = reader[data.Value.Alias],
                            Count = (int)reader[data.Count.Alias],
                        });
                }

                return record;
            });
        }

        public async Task UpdateRecords(TableViewCostElementInfo[] costBlockInfos, IEnumerable<TableViewRecord> records)
        {
            var queries = new List<SqlHelper>();
            var fieldDictionary = costBlockInfos.ToDictionary(
                info => info.Meta.Name, 
                info => new
                {
                    QueryInfo = info,
                    FieldsHashSet = new HashSet<string>(info.CostElementIds)
                });

            var recordIndex = 0;

            foreach (var record in records)
            {
                var whereCondition = ConditionHelper.And(
                    record.Coordinates.Select(
                        keyValue => SqlOperators.Equals(keyValue.Key, $"{keyValue.Key}_{recordIndex}", keyValue.Value.Id)));

                var groupedData =
                    record.Data.Select(keyValue => new { Column = this.ParseColumnAlias(keyValue.Key), Value = keyValue.Value })
                               .GroupBy(dataInfo => dataInfo.Column.TableName);

                foreach (var data in groupedData)
                {
                    if (fieldDictionary.TryGetValue(data.Key, out var info))
                    {
                        foreach (var dataInfo in data)
                        {
                            if (!info.FieldsHashSet.Contains(dataInfo.Column.Name))
                            {
                                throw new Exception($"Invalid column {dataInfo.Column.Name} from table {dataInfo.Column.TableName}");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Invalid table {data.Key}");
                    }

                    var updateColumns = data.Select(dataInfo => new ValueUpdateColumnInfo(
                        dataInfo.Column.Name, 
                        dataInfo.Value.Value, 
                        $"{dataInfo.Column.TableName}_{dataInfo.Column.Name}_{queries.Count}"));

                    var query = 
                        Sql.Update(info.QueryInfo.Meta, updateColumns.ToArray())
                           .Where(whereCondition);

                    queries.Add(query);
                }

                recordIndex++;
            }

            await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
        }

        public async Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(TableViewCostElementInfo[] costBlockInfos)
        {
            var result = new Dictionary<string, IEnumerable<NamedId>>();

            foreach (var costBlockInfo in costBlockInfos)
            {
                foreach (var costElementId in costBlockInfo.CostElementIds)
                {
                    if (costBlockInfo.Meta.CostElementsFields[costElementId] is ReferenceFieldMeta field)
                    {
                        var items = await this.sqlRepository.GetNameIdItems(field.ReferenceMeta, field.ReferenceValueField, field.ReferenceFaceField);
                        var key = this.BuildColumnAlias(costBlockInfo.Meta, field.Name);

                        result.Add(key, items);
                    }
                }
            }

            return result;
        }

        public TableViewRecordInfo GetTableViewRecordInfo(TableViewCostElementInfo[] costBlockInfos)
        {
            var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
            var columnInfo = this.BuildTableViewColumnInfo(costBlockInfos, coordinateFieldInfos);

            return new TableViewRecordInfo
            {
                Coordinates = columnInfo.CoordinateInfos.Select(this.CopyFieldInfo),
                Data = columnInfo.DataInfos.Select(this.CopyFieldInfo)
            };
        }

        private FieldInfo CopyFieldInfo(FieldInfo fieldInfo)
        {
            return new FieldInfo
            {
                DataIndex = fieldInfo.DataIndex,
                FieldName = fieldInfo.FieldName,
                MetaId = fieldInfo.MetaId
            };
        }

        private UnionSqlHelper BuildGetRecordsQuery(
            TableViewCostElementInfo[] costBlockInfos, 
            TableViewColumnInfo columnInfo, 
            IEnumerable<CoordinateFieldInfo> coordinateFieldInfos)
        {
            var coordinateDictionary = coordinateFieldInfos.ToDictionary(info => info.CoordinateField.Name);

            var costBlockQueryInfos = costBlockInfos.Select(costBlockInfo => new
            {
                FromMeta = costBlockInfo.Meta,
                SelectColumns = costBlockInfo.CostElementIds.SelectMany(costElementId => new[] 
                {
                    SqlFunctions.Max(costElementId, costBlockInfo.Meta.Name, costElementId) as BaseColumnInfo,
                    SqlFunctions.Count(
                        costElementId, 
                        true,
                        costBlockInfo.Meta.Name, 
                        this.BuildCountColumnAlias(costBlockInfo.Meta, costElementId)) as BaseColumnInfo
                }),
                GroupByColumns = costBlockInfo.Meta.CoordinateFields.Where(field => coordinateDictionary.ContainsKey(field.Name))
                                                                    .Select(field => new ColumnInfo(field.Name, costBlockInfo.Meta.Name))
                                                                    .ToArray()
            });

            var costBlockQueries = costBlockQueryInfos.Select(info => new
            {
                Meta = info.FromMeta,
                Query = Sql.Select(info.SelectColumns.Concat(info.GroupByColumns).ToArray())
                           .From(info.FromMeta)
                           .WhereNotDeleted(info.FromMeta)
                           .GroupBy(info.GroupByColumns)
                           .ToSqlBuilder()
            }).ToArray();

            var columns =
                columnInfo.CoordinateInfos.SelectMany(coordinate => new[] { coordinate.Id, coordinate.Name })
                                          .Concat(columnInfo.DataInfos.SelectMany(data => new[] { data.Value, data.Count }))
                                          .ToArray();

            var firstQuery = costBlockQueries[0];
            var joinQuery = Sql.Select(columns).FromQuery(firstQuery.Query, firstQuery.Meta.Name);
            var joinedCostBlocks = new List<CostBlockEntityMeta> { firstQuery.Meta };

            for (var index = 1; index < costBlockQueries.Length; index++)
            {
                var costBlockInfo = costBlockQueries[index];
                var conditions = new List<ConditionHelper>();

                foreach (var coordinateField in costBlockInfo.Meta.CoordinateFields)
                {
                    var conditionMeta = joinedCostBlocks.FirstOrDefault(
                        joinedCostBlock => 
                            coordinateDictionary.ContainsKey(coordinateField.Name) && 
                            joinedCostBlock.ContainsCoordinateField(coordinateField.Name));

                    if (conditionMeta != null)
                    {
                        conditions.Add(SqlOperators.Equals(
                            new ColumnInfo(coordinateField.Name, conditionMeta.Name),
                            new ColumnInfo(coordinateField.Name, costBlockInfo.Meta.Name)));
                    }
                }

                var query = new AliasSqlBuilder
                {
                    Alias = costBlockInfo.Meta.Name,
                    Query = new BracketsSqlBuilder
                    {
                        Query = costBlockInfo.Query
                    }
                };

                joinQuery = joinQuery.Join(query, ConditionHelper.And(conditions));

                joinedCostBlocks.Add(costBlockInfo.Meta);
            }

            var joinInfos = coordinateDictionary.Values.Select(info => new JoinInfo(info.Meta, info.CoordinateField.Name));
            var orderByColumns = columnInfo.CoordinateInfos.Select(info => info.Name).ToArray();

            return joinQuery.Join(joinInfos).OrderBy(SortDirection.Asc, orderByColumns);
        }

        private string BuildColumnAlias(BaseEntityMeta meta, string field)
        {
            return $"{meta.Name}{AliasSeparator}{field}";
        }

        private string BuildCountColumnAlias(BaseEntityMeta meta, string field)
        {
            return this.BuildColumnAlias(meta, $"{field}_Count");
        }

        private ColumnInfo ParseColumnAlias(string columnAlias)
        {
            var values = columnAlias.Split(AliasSeparator);

            return new ColumnInfo(values[1], values[0], columnAlias);
        }

        private TableViewColumnInfo BuildTableViewColumnInfo(IEnumerable<TableViewCostElementInfo> costBlockInfos, IEnumerable<CoordinateFieldInfo> coordinateFieldInfos)
        {
            var result = new TableViewColumnInfo();

            foreach (var info in coordinateFieldInfos)
            {
                var coordinateColumnInfo = new CoordinateColumnInfo
                {
                    MetaId = info.Meta.Name,
                    FieldName = info.CoordinateField.Name,
                    DataIndex = info.CoordinateField.Name,
                    Id = new ColumnInfo(info.CoordinateField.ReferenceValueField, info.CoordinateField.ReferenceMeta.Name, info.CoordinateField.Name),
                    Name = new ColumnInfo(info.CoordinateField.ReferenceFaceField, info.CoordinateField.ReferenceMeta.Name, $"{info.CoordinateField.Name}_Face")
                };

                result.CoordinateInfos.Add(coordinateColumnInfo);
            }

            foreach (var costBlockInfo in costBlockInfos)
            {
                foreach (var costElementId in costBlockInfo.CostElementIds)
                {
                    var countAlias = this.BuildCountColumnAlias(costBlockInfo.Meta, costElementId);
                    var dataIndex = this.BuildColumnAlias(costBlockInfo.Meta, costElementId);
                    var dataColumnInfo = new DataColumnInfo
                    {
                        MetaId = costBlockInfo.Meta.Name,
                        FieldName = costElementId,
                        DataIndex = dataIndex,
                        Value = new ColumnInfo(costElementId, costBlockInfo.Meta.Name, dataIndex),
                        Count = new ColumnInfo(countAlias, costBlockInfo.Meta.Name, countAlias)
                    };

                    result.DataInfos.Add(dataColumnInfo);
                }
            }

            return result;
        }

        private IEnumerable<CoordinateFieldInfo> GetCoordinateFieldInfos(TableViewCostElementInfo[] costBlockInfos)
        {
            var coordinateLists = costBlockInfos.Select(info => info.Meta.CoordinateFields.Select(field => field.Name)).ToArray();

            var fieldNames = coordinateLists[0];

            for (var index = 1; index < coordinateLists.Length; index++)
            {
                fieldNames = fieldNames.Intersect(coordinateLists[index]);
            }

            foreach (var fieldName in fieldNames)
            {
                ReferenceFieldMeta coordinateField;

                foreach (var costBlockInfo in costBlockInfos)
                {
                    coordinateField = (ReferenceFieldMeta)costBlockInfo.Meta.GetField(fieldName);

                    if (coordinateField != null)
                    {


                        yield return new CoordinateFieldInfo
                        {
                            Meta = costBlockInfo.Meta,
                            CoordinateField = coordinateField
                        };

                        break;
                    }
                }
            }
        }

        private class CoordinateFieldInfo
        {
            public CostBlockEntityMeta Meta { get; set; }

            public ReferenceFieldMeta CoordinateField { get; set; }
        }

        private class CoordinateColumnInfo : FieldInfo
        {
            public ColumnInfo Id { get; set; }

            public ColumnInfo Name { get; set; }
        }

        private class DataColumnInfo : FieldInfo
        {
            public ColumnInfo Value { get; set; }

            public ColumnInfo Count { get; set; }
        }

        private class TableViewColumnInfo
        {
            public List<CoordinateColumnInfo> CoordinateInfos { get; private set; }

            public List<DataColumnInfo> DataInfos { get; private set; }

            public TableViewColumnInfo()
            {
                this.CoordinateInfos = new List<CoordinateColumnInfo>();
                this.DataInfos = new List<DataColumnInfo>();
            }
        }
    }
}
