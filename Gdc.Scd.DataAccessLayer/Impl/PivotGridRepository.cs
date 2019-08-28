using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class PivotGridRepository : IPivotGridRepository
    {
        private readonly IRepositorySet repositorySet;

        public PivotGridRepository(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public async Task<PivotResult> GetData(PivotRequest request, BaseEntityMeta meta, SqlHelper customQuery = null)
        {
            var nonExistFields =
                request.GetAllAxisItems()
                       .Select(axisItem => axisItem.DataIndex)
                       .Except(meta.AllFields.Select(field => field.Name))
                       .ToArray();

            if (nonExistFields.Length > 0)
            {
                throw new Exception($"PivotRequest has non exist fields: {string.Join(",", nonExistFields)}");
            }

            var axisDictionary = new Dictionary<RequestAxisItem, Dictionary<string, ResultAxisItem>>();

            var query = this.BuildSql(request, meta, customQuery);
            var resultItems = await this.repositorySet.ReadBySql(query, MapRow);

            return new PivotResult
            {
                Success = true,
                LeftAxis = BuildResultAxisItems(request.LeftAxis),
                TopAxis = BuildResultAxisItems(request.TopAxis),
                Results = resultItems.ToArray()
            };

            ResultItem MapRow(IDataReader reader)
            {
                FillAxisDictionary(request.LeftAxis);
                FillAxisDictionary(request.TopAxis);

                return new ResultItem
                {
                    LeftKey = BuildKey(request.LeftAxis),
                    TopKey = BuildKey(request.TopAxis),
                    Values = request.Aggregate.ToDictionary(item => item.DataIndex, GetValueByAxisItem)
                };

                object GetValue(string dataIndex)
                {
                    var value = reader[dataIndex];

                    return DBNull.Value.Equals(value) ? null : value;
                }

                object GetValueByAxisItem(RequestAxisItem axisItem)
                {
                    return GetValue(axisItem.DataIndex);
                }

                string BuildKey(RequestAxisItem[] axisItems, bool isScrict = false)
                {
                    string result = null;

                    var axisValues =
                        axisItems.Select(GetValueByAxisItem)
                                 .Where(value => value != null)
                                 .ToArray();

                    if (!isScrict || axisItems.Length == axisValues.Length)
                    {
                        result = axisValues.Length == 0
                            ? request.GrandTotalKey
                            : string.Join(request.KeysSeparator, axisValues);
                    }

                    return result;
                }

                void FillAxisDictionary(RequestAxisItem[] axisItems)
                {
                    var axisItemInfos = axisItems.Select((axisItem, index) => (
                        axisItem, 
                        BuildKey(axisItems.Take(index + 1).ToArray(), true)));

                    foreach (var (axisItem, key) in axisItemInfos)
                    {
                        if (key != null)
                        {
                            if (!axisDictionary.TryGetValue(axisItem, out var dict))
                            {
                                dict = new Dictionary<string, ResultAxisItem>();

                                axisDictionary.Add(axisItem, dict);
                            }

                            if (!dict.ContainsKey(key))
                            {
                                dict.Add(key, new ResultAxisItem
                                {
                                    DimensionId = axisItem.Id,
                                    Key = key,
                                    Value = GetValueByAxisItem(axisItem)?.ToString(),
                                    Name = GetValue(this.BuildFaceValueColumn(axisItem.DataIndex))?.ToString()
                                });
                            }
                        }
                    }
                }
            }

            ResultAxisItem[] BuildResultAxisItems(IEnumerable<RequestAxisItem> axisItems)
            {
                return axisItems.SelectMany(item => axisDictionary[item].Values).ToArray();
            }
        }

        public async Task<DataTable> GetExportData(PivotRequest request, BaseEntityMeta meta, SqlHelper customQuery = null)
        {
            var query = this.BuildSql(request, meta, customQuery, true);
            var allAxisItems = request.GetAllAxisItems().ToArray();
            var dataTable = new DataTable();

            foreach (var item in allAxisItems.Concat(request.Aggregate))
            {
                dataTable.Columns.Add(item.Header);
            }

            var rows = await this.repositorySet.ReadBySql(query, reader =>
            {
                var row = dataTable.NewRow();

                foreach (var item in allAxisItems)
                {
                    var faceValueColumn = this.BuildFaceValueColumn(item.DataIndex);

                    row[item.Header] = reader[faceValueColumn];
                }

                foreach (var item in request.Aggregate)
                {
                    row[item.Header] = reader[item.DataIndex];
                }

                return row;
            });

            foreach (var row in rows)
            {
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private SqlHelper BuildSql(PivotRequest request, BaseEntityMeta meta, SqlHelper customQuery = null, bool groupnlyLowerLevel = false)
        {
            const string GroupedTableAlias = "Grouped";
            const int NullDimensionId = -1;

            var allAxisItems = request.GetAllAxisItems().ToArray();

            var referenceFields = allAxisItems.Select(item => meta.GetField(item.DataIndex)).OfType<ReferenceFieldMeta>().ToArray();
            var nullRefernceFields = referenceFields.Where(field => field.IsNullOption).ToDictionary(field => field.Name);
            var faceColumns =
                referenceFields.Select(field => new ColumnInfo(
                    field.ReferenceFaceField,
                    field.ReferenceMeta.Name,
                    this.BuildFaceValueColumn(field.Name)));
            
            var selectColumns =
                allAxisItems.Concat(request.Aggregate)
                            .Select(item => new ColumnInfo(item.DataIndex, GroupedTableAlias))
                            .Concat(faceColumns)
                            .ToArray();

            var joinInfos = 
                referenceFields.Except(nullRefernceFields.Values)
                               .Select(field => new JoinInfo(meta, field.Name, metaTableAlias: GroupedTableAlias)
                               {
                                   JoinType = JoinType.Left
                               });

            var query =
                Sql.Select(selectColumns)
                   .FromQuery(
                        BuildGroupedQueries(),
                        GroupedTableAlias)
                   .Join(joinInfos);

            if (nullRefernceFields.Count > 0)
            {
                foreach (var field in nullRefernceFields.Values)
                {
                    var joinQuery =
                        Sql.Select(
                                SqlFunctions.Value(NullDimensionId, field.ReferenceValueField), 
                                SqlFunctions.Value("Empty", field.ReferenceFaceField))
                           .Union(
                                Sql.Select(field.ReferenceValueField, field.ReferenceFaceField)
                                   .From(field.ReferenceMeta),
                                true);

                    query = 
                        query.JoinQuery(
                            joinQuery, 
                            SqlOperators.Equals(
                                new ColumnInfo(field.Name, GroupedTableAlias), 
                                new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name)),
                            field.ReferenceMeta.Name,
                            JoinType.Left);
                }
            }

            return query;

            SqlHelper BuildGroupedQueries()
            {
                SqlHelper result;

                if (groupnlyLowerLevel)
                {
                    var leftAxisColumnInfos = BuildColumnInfos(request.LeftAxis, request.LeftAxis.Length).ToArray();
                    var topAxisColumnInfos = BuildColumnInfos(request.TopAxis, request.TopAxis.Length).ToArray();

                    result = BuildGroupedQuery(leftAxisColumnInfos, topAxisColumnInfos);
                }
                else
                {
                    var groupedQueris = new List<SqlHelper>();

                    for (var leftAxisIndex = 1; leftAxisIndex <= request.LeftAxis.Length; leftAxisIndex++)
                    {
                        var leftAxisColumnInfos = BuildColumnInfos(request.LeftAxis, leftAxisIndex).ToArray();

                        for (var topAxisIndex = 1; topAxisIndex <= request.TopAxis.Length; topAxisIndex++)
                        {
                            var topAxisColumnInfos = BuildColumnInfos(request.TopAxis, topAxisIndex).ToArray();

                            groupedQueris.Add(BuildGroupedQuery(leftAxisColumnInfos, topAxisColumnInfos));
                        }
                    }

                    result = Sql.Union(groupedQueris, true);
                }

                return result;

                SqlHelper BuildGroupedQuery(IEnumerable<GroupedQueryColumnInfo> leftAxisColumnInfos, IEnumerable<GroupedQueryColumnInfo> topAxisColumnInfos)
                {
                    SqlHelper groupedQuery;

                    var leftTopAxisColumnInfos = leftAxisColumnInfos.Concat(topAxisColumnInfos).ToArray();
                    var leftTopAxisColumns = leftTopAxisColumnInfos.Select(info => info.Column).ToArray();
                    var groupedColumns = leftTopAxisColumns.Select(column => new ColumnInfo(column.Alias)).ToArray();

                    if (leftTopAxisColumnInfos.All(info => info.Type != ColumnType.NullReference))
                    {
                        groupedQuery =
                            Sql.Select(leftTopAxisColumns.Concat(BuildAggregateColumns()).ToArray())
                               .From(meta, customQuery)
                               .GroupBy(groupedColumns);
                    }
                    else
                    {
                        const string WithouotNullTable = "WithouotNullTable";

                        var columns =
                            leftTopAxisColumns.Select(column => new ColumnInfo(column.Alias, WithouotNullTable))
                                              .Cast<BaseColumnInfo>()
                                              .Concat(BuildAggregateColumns(WithouotNullTable))
                                              .ToArray();

                        groupedQuery =
                           Sql.Select(columns)
                              .FromQuery(
                                    Sql.Select(leftTopAxisColumns)
                                       .From(meta, customQuery),
                                    WithouotNullTable)
                              .GroupBy(groupedColumns);
                    }

                    return groupedQuery;

                    IEnumerable<QueryColumnInfo> BuildAggregateColumns(string tableName = null)
                    {
                        foreach (var aggregateItem in request.Aggregate)
                        {
                            switch (aggregateItem.Aggregator.ToUpper())
                            {
                                case "COUNT":
                                    yield return SqlFunctions.Count(alias: aggregateItem.DataIndex, tableName: tableName);
                                    break;

                                default:
                                    throw new NotSupportedException();
                            }
                        }
                    }
                }

                IEnumerable<GroupedQueryColumnInfo> BuildColumnInfos(RequestAxisItem[] axisItems, int count)
                {
                    for (var index = 0; index < count; index++)
                    {
                        var dataIndex = axisItems[index].DataIndex;

                        if (nullRefernceFields.TryGetValue(dataIndex, out var referenceField))
                        {
                            var column = SqlFunctions.IfElse(
                                dataIndex,
                                SqlOperators.IsNull(dataIndex),
                                new ValueSqlBuilder(NullDimensionId),
                                new ColumnSqlBuilder(dataIndex));

                            yield return new GroupedQueryColumnInfo
                            {
                                Column = column,
                                Type = ColumnType.NullReference
                            };
                        }
                        else
                        {
                            yield return new GroupedQueryColumnInfo
                            {
                                Column = new ColumnInfo(dataIndex, alias: dataIndex),
                                Type = ColumnType.Data
                            };
                        }
                    }

                    for (var index = count; index < axisItems.Length; index++)
                    {
                        yield return new GroupedQueryColumnInfo
                        {
                            Column = SqlFunctions.Value(null, axisItems[index].DataIndex),
                            Type = ColumnType.NullValue
                        };
                    }
                }
            }
        }

        private string BuildFaceValueColumn(string dataIndex)
        {
            return $"{dataIndex}_Face";
        }

        private class GroupedQueryColumnInfo
        {
            public BaseColumnInfo Column { get; set; }

            public ColumnType Type { get; set; }
        }

        private enum ColumnType
        {
            Data,
            NullValue,
            NullReference
        }
    }
}
