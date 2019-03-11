using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class TableViewRepository : ITableViewRepository
    {
        private const char AliasSeparator = '.';

        private readonly IRepositorySet repositorySet;

        private readonly ISqlRepository sqlRepository;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        public TableViewRepository(
            IRepositorySet repositorySet, 
            ISqlRepository sqlRepository, 
            ICostBlockFilterBuilder costBlockFilterBuilder)
        {
            this.repositorySet = repositorySet;
            this.sqlRepository = sqlRepository;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
        }

        public async Task<IEnumerable<Record>> GetRecords(CostElementInfo[] costElementInfos)
        {
            var coordinateMetas = this.GetCoordinateMetas(costElementInfos);
            var dependencyItems = await this.GetDependencyItems(costElementInfos);
            var queryInfo = this.BuildQueryInfo(costElementInfos, coordinateMetas, dependencyItems);
            var recordsQuery = this.BuildGetRecordsQuery(queryInfo);

            return await this.repositorySet.ReadBySql(recordsQuery, reader =>
            {
                var record = new Record();

                foreach (var coordinateInfo in queryInfo.CoordinateInfos)
                {
                    record.Coordinates.Add(
                        coordinateInfo.CoordinateMeta.Name,
                        new NamedId
                        {
                            Id = (long)reader[coordinateInfo.IdColumn],
                            Name = reader[coordinateInfo.NameColumn] as string
                        });

                    foreach (var additionalInfo in coordinateInfo.AdditionalDataInfos)
                    {
                        record.AdditionalData.Add(
                            additionalInfo.Data.DataIndex,
                            reader[additionalInfo.Data.DataIndex] as string);
                    }
                }

                foreach (var costBlockInfo in queryInfo.DataInfos)
                {
                    foreach (var costElementInfo in costBlockInfo.CostElementInfos)
                    {
                        record.Data.Add(
                            costElementInfo.DataIndex,
                            new ValueCount
                            {
                                Value = reader[costElementInfo.ValueColumn],
                                Count = (int)reader[costElementInfo.CountColumn],
                            });
                    }
                }

                return record;
            });
        }

        public async Task<IDictionary<string, ReferenceSet>> GetReferences(CostElementInfo[] costElementInfo)
        {
            var result = new Dictionary<string, ReferenceSet>();

            var groups =
                costElementInfo.SelectMany(info => info.CostElementIds.Select(costElementId => new { info.Meta, Field = info.Meta.CostElementsFields[costElementId] as ReferenceFieldMeta }))
                               .Where(info => info.Field != null)
                               .GroupBy(info => new { info.Field.ReferenceMeta, info.Field.ReferenceValueField, info.Field.ReferenceFaceField });

            foreach (var group in groups)
            {
                var items = await this.sqlRepository.GetNameIdItems(group.Key.ReferenceMeta, group.Key.ReferenceValueField, group.Key.ReferenceFaceField);

                foreach (var info in group)
                {
                    if (!result.TryGetValue(info.Meta.Name, out var referenceSet))
                    {
                        referenceSet = new ReferenceSet
                        {
                            References = new Dictionary<string, IEnumerable<NamedId>>()
                        };

                        result.Add(info.Meta.Name, referenceSet);
                    }

                    referenceSet.References.Add(info.Field.Name, items);
                }
            }

            return result;
        }

        public async Task<RecordInfo> GetRecordInfo(CostElementInfo[] costElementInfos)
        {
            var coordinateMetas = this.GetCoordinateMetas(costElementInfos);
            var dependencyItems = await this.GetDependencyItems(costElementInfos);
            var queryInfo = this.BuildQueryInfo(costElementInfos, coordinateMetas, dependencyItems);

            var dataInfos = 
                queryInfo.DataInfos.SelectMany(
                    costBlockInfo => costBlockInfo.CostElementInfos.Select(costElementInfo => costBlockInfo.BuildDataInfo(costElementInfo)));

            var additionalData =
                queryInfo.CoordinateInfos.SelectMany(
                    coordinateInfo => coordinateInfo.AdditionalDataInfos.Select(additionalDataInfo => additionalDataInfo.Data));

            return new RecordInfo
            {
                Coordinates = coordinateMetas.Select(meta => meta.Name).ToArray(),
                Data = dataInfos.ToArray(),
                AdditionalData = additionalData.ToArray()
            };
        }

        public async Task<IDictionary<string, IDictionary<long, NamedId>>> GetDependencyItems(CostElementInfo[] costElementInfos)
        {
            var result = new Dictionary<string, IDictionary<long, NamedId>>();

            var dependecyFields =
                costElementInfos.SelectMany(info => info.CostElementIds.Select(info.Meta.GetDomainDependencyField))
                                .Where(dependecyField => dependecyField != null)
                                .GroupBy(dependecyField => dependecyField.ReferenceMeta)
                                .Select(group => group.First());

            foreach (var dependecyField in dependecyFields)
            {
                var filter = this.costBlockFilterBuilder.BuildCoordinateItemsFilter(dependecyField.ReferenceMeta);
                var items = await this.sqlRepository.GetNameIdItems(dependecyField.ReferenceMeta, dependecyField.ReferenceValueField, dependecyField.ReferenceFaceField, filter);

                result.Add(dependecyField.ReferenceMeta.Name, items.ToDictionary(item => item.Id));
            }

            return result;
        }

        public IEnumerable<EditInfo> BuildEditInfos(CostElementInfo[] costElementInfos, IEnumerable<Record> records)
        {
            var coordinateValueCache = new Dictionary<long, long[]>();

            var queries = new List<SqlHelper>();
            var fieldDictionary = costElementInfos.ToDictionary(
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
                            EditFieldId = DeserializeDataIndex(keyValue.Key),
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

                    var coordinateGroups = editInfoGroup.GroupBy(rawEditInfo => 
                    {
                        string dependencyId = null;
                        long? dependencyItemId = null;

                        if (rawEditInfo.EditFieldId.DependencyItemId.HasValue)
                        {
                            var dependencyField = info.Meta.GetDomainDependencyField(rawEditInfo.EditFieldId.CostElementId);
                            if (dependencyField == null)
                            {
                                throw new Exception($"Invalid dependency '{rawEditInfo.EditFieldId.CostElementId}' from costblock '{rawEditInfo.EditFieldId.CostBlockId}'");
                            }

                            dependencyId = dependencyField.Name;
                            dependencyItemId = rawEditInfo.EditFieldId.DependencyItemId;
                        }

                        return new
                        {
                            rawEditInfo.Coordinates,
                            DependencyId = dependencyId,
                            DependencyItemId = dependencyItemId
                        };
                    });

                    foreach (var coordinateGroup in coordinateGroups)
                    {
                        foreach (var coordinate in coordinateGroup.Key.Coordinates)
                        {
                            if (!info.Meta.ContainsCoordinateField(coordinate.Key))
                            {
                                throw new Exception($"Invalid field '{coordinate.Key}' from costblock '{editInfoGroup.Key}'");
                            }
                        }

                        var coordinates = coordinateGroup.Key.Coordinates.ToDictionary(coord => coord.Key, coord => coord.Value.Id);

                        if (coordinateGroup.Key.DependencyItemId.HasValue)
                        {
                            coordinates.Add(coordinateGroup.Key.DependencyId, coordinateGroup.Key.DependencyItemId.Value);
                        }

                        valueInfos.Add(new ValuesInfo
                        {
                            CoordinateFilter = coordinates.ToDictionary(keyValue => keyValue.Key, keyValue => new [] { keyValue.Value }),
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

        private SqlHelper BuildGetRecordsQuery(QueryInfo queryInfo)
        {
            var costBlockQueryInfos = queryInfo.DataInfos.Select(costBlockInfo => 
            {
                IDictionary<string, IEnumerable<object>> filter = null;

                if (costBlockInfo is DependencyItemCostBlockQueryInfo dependencyItemQueryInfo)
                {
                    filter = new Dictionary<string, IEnumerable<object>>
                    {
                        [dependencyItemQueryInfo.DependecyField.Name] = new object[] { dependencyItemQueryInfo.DependencyItemId }
                    };
                }

                return new
                {
                    SelectColumns = costBlockInfo.CostElementInfos.SelectMany(costElementInfo => new[]
                    {
                        SqlFunctions.Max(costElementInfo.CostElementId, costBlockInfo.Alias, costElementInfo.ValueColumn) as BaseColumnInfo,
                        SqlFunctions.Count(costElementInfo.CostElementId, true, costBlockInfo.Alias, costElementInfo.CountColumn) as BaseColumnInfo
                    }),
                    From = new
                    {
                        Meta = costBlockInfo.Meta,
                        Alias = costBlockInfo.Alias
                    },
                    Filter = filter,
                    GroupByColumns = queryInfo.CoordinateInfos.Select(info => new ColumnInfo(info.CoordinateMeta.Name, costBlockInfo.Alias)).ToArray(),
                    TempTable = $"#{costBlockInfo.Alias}"
                };
            });

            var costBlockQueries = costBlockQueryInfos.Select(info => new
            {
                info.From,
                info.TempTable,
                Query = Sql.Select(info.SelectColumns.Concat(info.GroupByColumns).ToArray())
                           .Into(info.TempTable)
                           .From(info.From.Meta, info.From.Alias)
                           .WhereNotDeleted(info.From.Meta, info.Filter, info.From.Alias)
                           .GroupBy(info.GroupByColumns)
                           .ToSqlBuilder()
            }).ToArray();

            var coordinateIdColumns = new List<ColumnInfo>();
            var coordinateNameColumns = new List<ColumnInfo>();
            var additionalColumns = new List<ColumnInfo>();
            var additionalJoinInfos = new List<JoinInfo>();

            foreach (var info in queryInfo.CoordinateInfos)
            {
                coordinateIdColumns.Add(new ColumnInfo(info.CoordinateMeta.IdField.Name, info.CoordinateMeta.Name, info.IdColumn));
                coordinateNameColumns.Add(new ColumnInfo(info.CoordinateMeta.NameField.Name, info.CoordinateMeta.Name, info.NameColumn));

                foreach (var additionalDataInfo in info.AdditionalDataInfos)
                {
                    switch (additionalDataInfo.Field)
                    {
                        case SimpleFieldMeta simpleField:
                            additionalColumns.Add(new ColumnInfo(simpleField.Name, info.CoordinateMeta.Name, additionalDataInfo.Data.DataIndex));
                            break;

                        case ReferenceFieldMeta referenceField:
                            additionalColumns.Add(new ColumnInfo(referenceField.ReferenceFaceField, referenceField.ReferenceMeta.Name, additionalDataInfo.Data.DataIndex));
                            additionalJoinInfos.Add(new JoinInfo(info.CoordinateMeta, referenceField.Name)
                            {
                                JoinType = referenceField.IsNullOption ? JoinType.Left : JoinType.Inner
                            });
                            break;

                        default:
                            throw new NotImplementedException($"Support of type '{additionalDataInfo.Field.GetType()}' not implemented");
                    }
                }
            }

            var dataColumns = queryInfo.DataInfos.SelectMany(costBlockInfo => costBlockInfo.CostElementInfos.SelectMany(costElementInfo => new[] 
            {
                new ColumnInfo(costElementInfo.ValueColumn, costBlockInfo.Alias),
                new ColumnInfo(costElementInfo.CountColumn, costBlockInfo.Alias)
            }));

            var columns = coordinateIdColumns.Concat(coordinateNameColumns).Concat(dataColumns).Concat(additionalColumns).ToArray();

            var firstQuery = costBlockQueries[0];
            var joinQuery = Sql.Select(columns).From(firstQuery.TempTable, alias: firstQuery.From.Alias);

            for (var index = 1; index < costBlockQueries.Length; index++)
            {
                var costBlockQuery = costBlockQueries[index];

                var conditions = 
                    queryInfo.CoordinateInfos.Select(
                        info => SqlOperators.Equals(
                            new ColumnInfo(info.CoordinateMeta.Name, firstQuery.From.Alias),
                            new ColumnInfo(info.CoordinateMeta.Name, costBlockQuery.From.Alias)));

                joinQuery = joinQuery.Join(costBlockQuery.TempTable, ConditionHelper.And(conditions), JoinType.Inner, costBlockQuery.From.Alias);
            }

            var joinInfos = 
                queryInfo.CoordinateInfos.Select(info => new JoinInfo(firstQuery.From.Meta, info.CoordinateMeta.Name, null, firstQuery.From.Alias))
                                         .Concat(additionalJoinInfos);

            var commonQuery = joinQuery.Join(joinInfos).OrderBy(SortDirection.Asc, coordinateNameColumns.ToArray()).ToSqlBuilder();

            var createTaleQueries = new List<ISqlBuilder>();
            var dropTableQueries = new List<ISqlBuilder>();

            foreach(var info in costBlockQueries)
            {
                createTaleQueries.Add(info.Query);
                dropTableQueries.Add(Sql.DropTable(info.TempTable).ToSqlBuilder());
            }

            var queryList = new List<ISqlBuilder>(createTaleQueries)
            {
                commonQuery
            };

            queryList.AddRange(dropTableQueries);

            return Sql.Queries(queryList);
        }

        private static string SerializeDataIndex(string costBlockId, string costElementId, long? dependencyItemId = null)
        {
            var result = $"{costBlockId}{AliasSeparator}{costElementId}";

            if (dependencyItemId.HasValue)
            {
                result = $"{result}{AliasSeparator}{dependencyItemId}";
            }

            return result;
        }

        private static (string CostBlockId, string CostElementId, long? DependencyItemId) DeserializeDataIndex(string value)
        {
            var values = value.Split(AliasSeparator);
            var costBlockId = values[0];
            var costElementId = values[1];

            long? dependencyItemId = null;

            if (values.Length > 2)
            {
                dependencyItemId = long.Parse(values[2]);
            }

            return (costBlockId, costElementId, dependencyItemId);
        }

        private QueryInfo BuildQueryInfo(
            CostElementInfo[] costElementInfos, 
            IEnumerable<NamedEntityMeta> coordinateMetas,
            IDictionary<string, IDictionary<long, NamedId>> dependencyItems)
        {
            var result = new QueryInfo
            {
                DataInfos = new List<CostBlockQueryInfo>(),
                CoordinateInfos = coordinateMetas.Select(meta => new CoordinateColumnInfo
                {
                    CoordinateMeta = meta,
                    IdColumn = $"{meta.Name}_Id",
                    NameColumn = $"{meta.Name}_Name",
                    AdditionalDataInfos = this.BuildAdditionalDataInfos(meta)
                }).ToList()
            };

            foreach (var costElementInfo in costElementInfos)
            {
                var dependencyGroups =
                    costElementInfo.CostElementIds.GroupBy(costElementId => costElementInfo.Meta.GetDomainDependencyField(costElementId));

                foreach (var dependencyGroup in dependencyGroups)
                {
                    if (dependencyGroup.Key == null)
                    {
                        var costElementQueryInfos = 
                            dependencyGroup.Select(
                                costElementId => BuildDataColumnInfo(
                                    costElementInfo.Meta, 
                                    costElementId, 
                                    costElementId, 
                                    SerializeDataIndex(costElementInfo.Meta.CostBlockId, costElementId)));

                        result.DataInfos.Add(new CostBlockQueryInfo
                        {
                            Meta = costElementInfo.Meta,
                            Alias = costElementInfo.Meta.Name,
                            CostElementInfos = costElementQueryInfos.ToArray()
                        });
                    }
                    else
                    {
                        foreach (var item in dependencyItems[dependencyGroup.Key.Name].Values.OrderBy(x => x.Name))
                        {
                            var costElementQueryInfos =
                                dependencyGroup.Select(
                                    costElementId => BuildDataColumnInfo(
                                        costElementInfo.Meta,
                                        costElementId,
                                        $"{costElementId}_{item.Id}",
                                        SerializeDataIndex(costElementInfo.Meta.CostBlockId, costElementId, item.Id)));

                            result.DataInfos.Add(new DependencyItemCostBlockQueryInfo
                            {
                                Meta = costElementInfo.Meta,
                                Alias = $"{costElementInfo.Meta.Name}_{dependencyGroup.Key.ReferenceMeta.Name}_{item.Id}",
                                DependencyItemId = item.Id,
                                DependecyField = dependencyGroup.Key,
                                CostElementInfos = costElementQueryInfos.ToArray()
                            });
                        }
                    }
                }
            }

            return result;

            DataColumnInfo BuildDataColumnInfo(CostBlockEntityMeta meta, string costElementId, string name, string dataIndex)
            {
                return new DataColumnInfo
                {
                    CostElementId = costElementId,
                    DataIndex = dataIndex,
                    ValueColumn = $"{meta.Name}_{name}_value",
                    CountColumn = $"{meta.Name}_{name}_count"
                };
            }
        }

        private IEnumerable<AdditionalDataInfo> BuildAdditionalDataInfos(NamedEntityMeta coordinateMeta)
        {
            IEnumerable<AdditionalDataInfo> result;

            switch (coordinateMeta)
            {
                case WgEnityMeta wgMeta:
                    result = new[]
                    {
                        new AdditionalDataInfo
                        {
                            Field = wgMeta.PlaField,
                            Data = new AdditionalData
                            {
                                DataIndex = "Pla",
                                Title = "PLA"
                            }
                        },
                        new AdditionalDataInfo
                        {
                            Field = wgMeta.DescriptionField,
                            Data = new AdditionalData
                            {
                                DataIndex = wgMeta.DescriptionField.Name,
                                Title = "WG Full name"
                            }
                        }
                    };
                    break;

                default:
                    result = Enumerable.Empty<AdditionalDataInfo>();
                    break;

            }

            return result;
        }

        private IEnumerable<NamedEntityMeta> GetCoordinateMetas(CostElementInfo[] costElementInfos)
        {
            var coordinateLists = 
                costElementInfos.Select(info => info.Meta.InputLevelFields.Select(field => field.ReferenceMeta).OfType<NamedEntityMeta>())
                                .ToArray();

            var metas = coordinateLists.Count() > 0 ? coordinateLists[0].Where(coord=>coord.Name== MetaConstants.WgInputLevelName) : Enumerable.Empty<NamedEntityMeta>();

            return metas;
        }

        private class AdditionalDataInfo
        {
            public FieldMeta Field { get; set; }

            public AdditionalData Data { get; set; }
        }

        private class CoordinateColumnInfo 
        {
            public NamedEntityMeta CoordinateMeta { get; set; }

            public string IdColumn { get; set; }

            public string NameColumn { get; set; }

            public IEnumerable<AdditionalDataInfo> AdditionalDataInfos { get; set; }
        }

        private class DataColumnInfo
        {
            public string CostElementId { get; set; }

            public string DataIndex { get; set; }

            public string ValueColumn { get; set; }

            public string CountColumn { get; set; }
        }

        private class CostBlockQueryInfo
        {
            public CostBlockEntityMeta Meta { get; set; }

            public IEnumerable<DataColumnInfo> CostElementInfos { get; set; }

            public string Alias { get; set; }

            public virtual DataInfo BuildDataInfo(DataColumnInfo dataColumnInfo)
            {
                return new DataInfo
                {
                    ApplicationId = this.Meta.ApplicationId,
                    CostBlockId = this.Meta.CostBlockId,
                    CostElementId = dataColumnInfo.CostElementId,
                    DataIndex = this.GetDataIndex(dataColumnInfo)
                };
            }

            protected virtual string GetDataIndex(DataColumnInfo dataColumnInfo)
            {
                return SerializeDataIndex(this.Meta.CostBlockId, dataColumnInfo.CostElementId);
            }
        }

        private class DependencyItemCostBlockQueryInfo : CostBlockQueryInfo
        {
            public ReferenceFieldMeta DependecyField { get; set; }

            public long? DependencyItemId { get; set; }

            public override DataInfo BuildDataInfo(DataColumnInfo dataColumnInfo)
            {
                var dataInfo = base.BuildDataInfo(dataColumnInfo);

                dataInfo.DependencyItemId = this.DependencyItemId;

                return dataInfo;
            }

            protected override string GetDataIndex(DataColumnInfo dataColumnInfo)
            {
                return SerializeDataIndex(this.Meta.CostBlockId, dataColumnInfo.CostElementId, this.DependencyItemId);
            }
        }

        private class QueryInfo
        {
            public List<CoordinateColumnInfo> CoordinateInfos { get; set; }

            public List<CostBlockQueryInfo> DataInfos { get; set; }
        }
    }
}
