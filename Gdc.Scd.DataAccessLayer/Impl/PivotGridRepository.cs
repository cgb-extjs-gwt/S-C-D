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

        private SqlHelper BuildSql(PivotRequest request, BaseEntityMeta meta, SqlHelper customQuery = null)
        {
            const string GroupedTableAlias = "Grouped";

            var allAxisItems = request.GetAllAxisItems().ToArray();

            var referenceFields = allAxisItems.Select(item => meta.GetField(item.DataIndex)).OfType<ReferenceFieldMeta>().ToArray();
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

            var joinInfos = referenceFields.Select(field => new JoinInfo(meta, field.Name, metaTableAlias: GroupedTableAlias)
            {
                JoinType = JoinType.Left
            });

            return
                Sql.Select(selectColumns)
                   .FromQuery(
                        Sql.Union(BuildGroupedQueries(), true),
                        GroupedTableAlias)
                   .Join(joinInfos);

            IEnumerable<SqlHelper> BuildGroupedQueries()
            {
                var aggregateColumns = request.Aggregate.Select(BuildAggregateColumn).ToArray();

                for (var leftAxisIndex = 1; leftAxisIndex <= request.LeftAxis.Length; leftAxisIndex++)
                {
                    var leftAxisColumns = BuildColumns(request.LeftAxis, leftAxisIndex).ToArray();

                    for (var topAxisIndex = 1; topAxisIndex <= request.TopAxis.Length; topAxisIndex++)
                    {
                        var topAxisColumns = BuildColumns(request.TopAxis, topAxisIndex).ToArray();

                        yield return
                            Sql.Select(leftAxisColumns.Concat(topAxisColumns).Concat(aggregateColumns).ToArray())
                               .From(meta, customQuery)
                               .GroupBy(leftAxisColumns.Concat(topAxisColumns).OfType<ColumnInfo>().ToArray());
                    }
                }

                IEnumerable<BaseColumnInfo> BuildColumns(RequestAxisItem[] axisItems, int count)
                {
                    for (var index = 0; index < count; index++)
                    {
                        var dataIndex = axisItems[index].DataIndex;

                        yield return new ColumnInfo(dataIndex, alias: dataIndex);
                    }

                    for (var index = count; index < axisItems.Length; index++)
                    {
                        var dataIndex = axisItems[index].DataIndex;

                        yield return new QueryColumnInfo(
                            new RawSqlBuilder("null"),
                            dataIndex);
                    }
                }

                QueryColumnInfo BuildAggregateColumn(RequestAxisItem aggregateItem)
                {
                    QueryColumnInfo result = null;

                    switch (aggregateItem.Aggregator.ToUpper())
                    {
                        case "COUNT":
                            result = SqlFunctions.Count(alias: aggregateItem.DataIndex);
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    return result;
                }
            }
        }

        private string BuildFaceValueColumn(string dataIndex)
        {
            return $"{dataIndex}_Face";
        }
    }
}
