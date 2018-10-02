using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
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

        public async Task<DataInfo<TableViewRecord>> GetRecords(
            TableViewCostElementInfo[] costBlockInfos, 
            QueryInfo queryInfo, 
            IDictionary<ColumnInfo, IEnumerable<object>> filter = null)
        {
            var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
            var columnInfo = this.BuildTableViewColumnInfo(costBlockInfos, coordinateFieldInfos);
            var columns =
                columnInfo.CoordinateInfos.SelectMany(coordinate => new[] { coordinate.Id, coordinate.Name })
                                            .Concat(columnInfo.DataInfos.SelectMany(data => new[] { data.Value, data.Count }))
                                            .ToArray();

            if (queryInfo.Sort == null)
            {
                queryInfo.Sort = new SortInfo
                {
                    Direction = SortDirection.Asc,
                    Property = columns[0].Alias
                };
            }

            var groupByColumns = columnInfo.GroupByColumns.ToArray();
            var recordsQuery = 
                this.BuildGetRecordsQuery(costBlockInfos, coordinateFieldInfos, filter, groupByColumns, columns)
                    .ByQueryInfo(queryInfo);

            var records = await this.repositorySet.ReadBySql(recordsQuery, reader =>
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

                //var record = new TableViewRecord();

                //foreach (var column in columnInfo.CoordinateIdColumns)
                //{
                //    record.Coordinates.Add(column.Alias, (long)reader[column.Alias]);
                //}

                //foreach (var column in columnInfo.DataValueColumns)
                //{
                //    record.Data.Add(column.Alias, reader[column.Alias]);
                //}

                //return record;
            });

            var countColumn = SqlFunctions.Count();
            var countQuery = this.BuildGetRecordsQuery(costBlockInfos, coordinateFieldInfos, filter, groupByColumns, countColumn);

            var count = (await this.repositorySet.ReadBySql(countQuery, reader => reader.GetInt32(0))).First();

            return new DataInfo<TableViewRecord>
            {
                Items = records,
                Total = count
            };
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

            foreach (var record in records)
            {
                var coordinateIds =
                    record.Coordinates.Select(keyValue => new { Column = this.ParseColumnAlias(keyValue.Key), Id = keyValue.Value })
                                      .ToDictionary(idInfo => idInfo.Column.TableName);

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
                        dataInfo.Value, 
                        $"{dataInfo.Column.TableName}_{dataInfo.Column.Name}_{queries.Count}"));

                    var coordinateInfo = coordinateIds[data.Key];

                    var query = 
                        Sql.Update(info.QueryInfo.Meta, updateColumns.ToArray())
                           .Where(SqlOperators.Equals(coordinateInfo.Column.Name, coordinateInfo.Column.Name, coordinateInfo.Id, coordinateInfo.Column.TableName));

                    queries.Add(query);
                }
            }

            await this.repositorySet.ExecuteSqlAsync(Sql.Queries(queries));
        }

        public async Task<IDictionary<string, IEnumerable<NamedId>>> GetFilters(TableViewCostElementInfo[] costBlockInfos)
        {
            var result = new Dictionary<string, IEnumerable<NamedId>>();
            var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);

            foreach (var fielInfo in coordinateFieldInfos)
            {
                var field = fielInfo.CoordinateField;
                var items = await this.sqlRepository.GetNameIdItems(field.ReferenceMeta, field.ReferenceValueField, field.ReferenceFaceField);
                var key = this.BuildColumnAlias(fielInfo.Meta, fielInfo.CoordinateField.Name);

                result.Add(key, items);
            }

            return result;
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

        private OrderBySqlHelper BuildGetRecordsQuery(
            TableViewCostElementInfo[] costBlockInfos, 
            IEnumerable<CoordinateFieldInfo> coordinateFieldInfos,
            IDictionary<ColumnInfo, IEnumerable<object>> filter,
            ColumnInfo[] groupByColumns,
            params BaseColumnInfo[] columns)
        {
            var firstCostBlockInfo = costBlockInfos[0];
            var joinQuery = Sql.Select(columns).From(firstCostBlockInfo.Meta);
            var joinedCostBlocks = new List<TableViewCostElementInfo> { firstCostBlockInfo };

            for (var index = 1; index < costBlockInfos.Length; index++)
            {
                var costBlockInfo = costBlockInfos[index];
                var conditions = new List<ConditionHelper>();

                foreach (var coordinateField in costBlockInfo.Meta.CoordinateFields)
                {
                    var conditionInfo = joinedCostBlocks.FirstOrDefault(
                        joinedCostBlock => joinedCostBlock.Meta.ContainsCoordinateField(coordinateField.Name));

                    if (conditionInfo != null)
                    {
                        conditions.Add(SqlOperators.Equals(
                            new ColumnInfo(coordinateField.Name, conditionInfo.Meta.Name),
                            new ColumnInfo(coordinateField.Name, costBlockInfo.Meta.Name)));
                    }
                }

                joinQuery = joinQuery.Join(costBlockInfo.Meta, ConditionHelper.And(conditions));

                joinedCostBlocks.Add(costBlockInfo);
            }

            var joinInfos = coordinateFieldInfos.Select(info => new JoinInfo(info.Meta, info.CoordinateField.Name));
            var query = joinQuery.Join(joinInfos).Where(filter);

            return 
                groupByColumns == null || groupByColumns.Length == 0 
                    ? query 
                    : query.GroupBy(groupByColumns);
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
            //var result = new TableViewColumnInfo();

            //var groupByInfo = costBlockInfos.FirstOrDefault(info => info.Meta.InputLevelFields[MetaConstants.CountryInputLevelName] != null);
            //if (groupByInfo != null)
            //{
            //    result.GroupByColumns = new[]
            //    {
            //        new ColumnInfo(MetaConstants.CountryInputLevelName, groupByInfo.Meta.Name)
            //    };
            //}

            var result = new TableViewColumnInfo();

            foreach (var info in coordinateFieldInfos)
            {
                var coordinateColumnInfo = this.BuildFieldInfo<CoordinateColumnInfo>(info.Meta, info.CoordinateField.Name);

                coordinateColumnInfo.Id = new ColumnInfo(info.CoordinateField.ReferenceValueField, info.CoordinateField.ReferenceMeta.Name, info.CoordinateField.Name);
                coordinateColumnInfo.Name = new ColumnInfo(info.CoordinateField.ReferenceFaceField, info.CoordinateField.ReferenceMeta.Name, $"{info.CoordinateField.Name}_Face");

                result.CoordinateInfos.Add(coordinateColumnInfo);
                result.GroupByColumns.Add(coordinateColumnInfo.Id);
                result.GroupByColumns.Add(coordinateColumnInfo.Name);
            }

            //if (result.GroupByColumns == null)
            //{
            //    foreach (var costBlockInfo in costBlockInfos)
            //    {
            //        foreach (var costElementId in costBlockInfo.CostElementIds)
            //        {
            //            var dataColumnInfo = this.BuildFieldInfo<DataColumnInfo>(costBlockInfo.Meta, costElementId);

            //            dataColumnInfo.Value = new ColumnInfo(costElementId, costBlockInfo.Meta.Name, dataColumnInfo.DataIndex);
            //            dataColumnInfo.Count = new QueryColumnInfo(
            //                new RawSqlBuilder { RawSql = "1" },
            //                this.BuildCountColumnAlias(costBlockInfo.Meta, costElementId));

            //            result.DataInfos.Add(dataColumnInfo);
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var costBlockInfo in costBlockInfos)
            //    {
            //        foreach (var costElementId in costBlockInfo.CostElementIds)
            //        {
            //            var dataColumnInfo = this.BuildFieldInfo<DataColumnInfo>(costBlockInfo.Meta, costElementId);

            //            dataColumnInfo.Value = SqlFunctions.Max(costElementId, tableName: costBlockInfo.Meta.Name, alias: dataColumnInfo.DataIndex);
            //            dataColumnInfo.Count = SqlFunctions.Count(
            //                costElementId,
            //                tableName: costBlockInfo.Meta.Name,
            //                alias: this.BuildCountColumnAlias(costBlockInfo.Meta, costElementId));

            //            result.DataInfos.Add(dataColumnInfo);
            //        }
            //    }
            //}

            foreach (var costBlockInfo in costBlockInfos)
            {
                foreach (var costElementId in costBlockInfo.CostElementIds)
                {
                    var dataColumnInfo = this.BuildFieldInfo<DataColumnInfo>(costBlockInfo.Meta, costElementId);

                    dataColumnInfo.Value = SqlFunctions.Max(costElementId, tableName: costBlockInfo.Meta.Name, alias: dataColumnInfo.DataIndex);
                    dataColumnInfo.Count = SqlFunctions.Count(
                        costElementId,
                        tableName: costBlockInfo.Meta.Name,
                        alias: this.BuildCountColumnAlias(costBlockInfo.Meta, costElementId));

                    result.DataInfos.Add(dataColumnInfo);
                }
            }

            return result;


            //var coordinateColumns = coordinateFieldInfos.Select(
            //    info => new ColumnInfo(info.CoordinateField.ReferenceFaceField, info.CoordinateField.ReferenceMeta.Name, info.CoordinateField.Name));

            //var costElementColumns = new List<ColumnInfo>();
            //var coordinateIdColumns = new List<ColumnInfo>();

            //foreach (var info in costBlockInfos)
            //{
            //    costElementColumns.AddRange(
            //        info.CostElementIds.Select(info.Meta.GetField)
            //                           .Select(field => new ColumnInfo(
            //                               field.Name, 
            //                               info.Meta.Name, 
            //                               this.BuildColumnAlias(info.Meta, field))));

            //    coordinateIdColumns.Add(new ColumnInfo(info.Meta.IdField.Name, info.Meta.Name, this.BuildColumnAlias(info.Meta, info.Meta.IdField)));
            //}

            //return new TableViewColumnInfo
            //{
            //    CoordinateIdColumns = coordinateIdColumns.ToArray(),
            //    DataValueColumns = coordinateColumns.Concat(costElementColumns).ToArray()
            //};
        }

        private T BuildFieldInfo<T>(BaseEntityMeta meta, string fieldName) where T : FieldInfo, new()
        {
            return new T
            {
                MetaId = meta.Name,
                FieldName = fieldName,
                DataIndex = this.BuildColumnAlias(meta, fieldName)
            };
        }

        private IEnumerable<CoordinateFieldInfo> GetCoordinateFieldInfos(IEnumerable<TableViewCostElementInfo> costBlockInfos)
        {
            var coordinateHashSet = new HashSet<string>();
            var coordinateFieldInfos = new List<CoordinateFieldInfo>();

            foreach (var info in costBlockInfos)
            {
                var coordinateFields = info.Meta.CoordinateFields.Where(x => x.Name != MetaConstants.CountryInputLevelName);

                foreach (var field in coordinateFields)
                {
                    if (!coordinateHashSet.Contains(field.Name))
                    {
                        coordinateHashSet.Add(field.Name);
                        coordinateFieldInfos.Add(new CoordinateFieldInfo
                        {
                            Meta = info.Meta,
                            CoordinateField = field
                        });
                    }
                }
            }

            return coordinateFieldInfos;
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

        //private abstract class BaseTableViewColumnInfo
        //{
        //    public string MetaId { get; set; }

        //    public string FieldName { get; set; }
        //}

        private class DataColumnInfo : FieldInfo
        {
            public BaseColumnInfo Value { get; set; }

            public QueryColumnInfo Count { get; set; }
        }

        private class TableViewColumnInfo
        {
            public List<CoordinateColumnInfo> CoordinateInfos { get; private set; }

            public List<DataColumnInfo> DataInfos { get; private set; }

            public List<ColumnInfo> GroupByColumns { get; private set; }

            public TableViewColumnInfo()
            {
                this.CoordinateInfos = new List<CoordinateColumnInfo>();
                this.DataInfos = new List<DataColumnInfo>();
                this.GroupByColumns = new List<ColumnInfo>();
            }
        }
    }
}
