using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;
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

        public async Task<IEnumerable<Record>> GetRecords(CostElementInfo[] costBlockInfos)
        {
            var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
            var columnInfo = this.BuildTableViewColumnInfo(costBlockInfos, coordinateFieldInfos);
            var recordsQuery = this.BuildGetRecordsQuery(costBlockInfos, columnInfo, coordinateFieldInfos);

            return await this.repositorySet.ReadBySql(recordsQuery, reader =>
            {
                var record = new Record();

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

        public async Task UpdateRecords(IEnumerable<EditInfo> editInfos)
        {
            var queries = new List<SqlHelper>();
            var paramIndex = 0;

            foreach (var editInfo in editInfos)
            {
                foreach (var valueInfo in editInfo.ValueInfos)
                {
                    var updateColumns = valueInfo.Values.Select(costElementValue => new ValueUpdateColumnInfo(
                        costElementValue.Key,
                        costElementValue.Value,
                        $"param_{paramIndex++}"));

                    var whereCondition = ConditionHelper.And(
                        valueInfo.Coordinates.Select(
                            coordinate => SqlOperators.Equals(coordinate.Key, $"param_{paramIndex++}", coordinate.Value)));

                    var query =
                        Sql.Update(editInfo.Meta, updateColumns.ToArray())
                           .Where(whereCondition);

                    queries.Add(query);
                }
            }

            await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
        }

        public async Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(CostElementInfo[] costBlockInfos)
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

        public RecordInfo GetTableViewRecordInfo(CostElementInfo[] costBlockInfos)
        {
            var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
            var columnInfo = this.BuildTableViewColumnInfo(costBlockInfos, coordinateFieldInfos);

            return new RecordInfo
            {
                Coordinates = columnInfo.CoordinateInfos.Select(this.CopyFieldInfo),
                Data = columnInfo.DataInfos.Select(this.CopyFieldInfo)
            };
        }

        public IEnumerable<EditInfo> BuildEditInfos(CostElementInfo[] costBlockInfos, IEnumerable<Record> records)
        {
            var queries = new List<SqlHelper>();
            var fieldDictionary = costBlockInfos.ToDictionary(
                info => info.Meta.Name,
                info => new
                {
                    Meta = info.Meta,
                    FieldsHashSet = new HashSet<string>(info.CostElementIds)
                });

            var editInfoGroups =
                records.SelectMany(
                    record =>
                        record.Data.Select(keyValue => new
                        {
                            record.Coordinates,
                            EditFieldId = this.DeserializeCostElementId(keyValue.Key),
                            Value = keyValue.Value.Value,
                        }))
                      .GroupBy(editInfo => editInfo.EditFieldId.CostBlockId);

            foreach(var editInfoGroup in editInfoGroups)
            {
                if (fieldDictionary.TryGetValue(editInfoGroup.Key, out var info))
                {
                    foreach (var rawEditInfo in editInfoGroup)
                    {
                        if (!info.FieldsHashSet.Contains(rawEditInfo.EditFieldId.CostElementId))
                        {
                            throw new Exception($"Invalid cost element '{rawEditInfo.EditFieldId.CostElementId}' from costblock '{rawEditInfo.EditFieldId.CostBlockId}'");
                        }
                    }

                    var valueInfos = new List<ValuesInfo>();

                    foreach (var coordinateGroup in editInfoGroup.GroupBy(rawEditInfo => rawEditInfo.Coordinates))
                    {
                        foreach (var coordinate in coordinateGroup.Key)
                        {
                            if (!info.Meta.ContainsCoordinateField(coordinate.Key))
                            {
                                throw new Exception($"Invalid field '{coordinate.Key}' from costblock '{editInfoGroup.Key}'");
                            }
                        }

                        valueInfos.Add(new ValuesInfo
                        {
                            Coordinates = coordinateGroup.Key.ToDictionary(coord => coord.Key, coord => coord.Value.Id),
                            Values = coordinateGroup.ToDictionary(rawEditInfo => rawEditInfo.EditFieldId.CostElementId, rawEditInfo => rawEditInfo.Value)
                        });
                    }

                    yield return new EditInfo
                    {
                        Meta = info.Meta,
                        ValueInfos = valueInfos
                    };
                }
                else
                {
                    throw new Exception($"Invalid table {editInfoGroup.Key}");
                }
            }
        }

        private FieldInfo CopyFieldInfo(FieldInfo fieldInfo)
        {
            return new FieldInfo
            {
                DataIndex = fieldInfo.DataIndex,
                FieldName = fieldInfo.FieldName,
                MetaId = fieldInfo.MetaId,
                SchemaId = fieldInfo.SchemaId
            };
        }

        private UnionSqlHelper BuildGetRecordsQuery(
            CostElementInfo[] costBlockInfos, 
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
            return this.SerializeCostElementId(meta.Name, field);
        }

        private string BuildCountColumnAlias(BaseEntityMeta meta, string field)
        {
            return this.BuildColumnAlias(meta, $"{field}_Count");
        }

        private string SerializeCostElementId(string costBlockId, string costElementId)
        {
            return $"{costBlockId}{AliasSeparator}{costElementId}";
        }

        private (string CostBlockId, string CostElementId) DeserializeCostElementId(string value)
        {
            var values = value.Split(AliasSeparator);

            return (values[0], values[1]);
        }

        private TableViewColumnInfo BuildTableViewColumnInfo(IEnumerable<CostElementInfo> costBlockInfos, IEnumerable<CoordinateFieldInfo> coordinateFieldInfos)
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
                        SchemaId = costBlockInfo.Meta.Schema,
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

        private IEnumerable<CoordinateFieldInfo> GetCoordinateFieldInfos(CostElementInfo[] costBlockInfos)
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
