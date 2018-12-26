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

        public async Task<IEnumerable<Record>> GetRecords(CostElementInfo[] costElementInfos)
        {
            //var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
            var coordinateMetas = this.GetCoordinateMetas(costElementInfos);
            var dependencyItems = await this.GetDependencyItems(costElementInfos);
            var queryInfo = this.BuildQueryInfo(costElementInfos, coordinateMetas, dependencyItems);
            var recordsQuery = this.BuildGetRecordsQuery(queryInfo);

            //return await this.repositorySet.ReadBySql(recordsQuery, reader =>
            //{
            //    var record = new Record();

            //    foreach (var coordinate in queryInfo.CoordinateInfos)
            //    {
            //        record.Coordinates.Add(
            //            coordinate.Id.Alias,
            //            new NamedId
            //            {
            //                Id = (long)reader[coordinate.Id.Alias],
            //                Name = (string)reader[coordinate.Name.Alias]
            //            });
            //    }

            //    foreach (var data in queryInfo.DataInfos)
            //    {
            //        record.Data.Add(
            //            data.Value.Alias,
            //            new ValueCount
            //            {
            //                Value = reader[data.Value.Alias],
            //                Count = (int)reader[data.Count.Alias],
            //            });
            //    }

            //    return record;
            //});

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
                            Name = (string)reader[coordinateInfo.NameColumn]
                        });
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

        //public async Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(CostElementInfo[] costElementInfo)
        //{
        //    var result = new Dictionary<string, IEnumerable<NamedId>>();

        //    foreach (var costBlockInfo in costElementInfo)
        //    {
        //        foreach (var costElementId in costBlockInfo.CostElementIds)
        //        {
        //            if (costBlockInfo.Meta.CostElementsFields[costElementId] is ReferenceFieldMeta field)
        //            {
        //                var items = await this.sqlRepository.GetNameIdItems(field.ReferenceMeta, field.ReferenceValueField, field.ReferenceFaceField);

        //                result.Add(field.ReferenceMeta.Name, items);
        //            }
        //        }
        //    }

        //    return result;
        //}

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

        //public RecordInfo GetRecordInfo(CostElementInfo[] costBlockInfos)
        //{
        //    var coordinateFieldInfos = this.GetCoordinateFieldInfos(costBlockInfos);
        //    var columnInfo = this.BuildTableViewColumnInfo(costBlockInfos, coordinateFieldInfos);

        //    return new RecordInfo
        //    {
        //        Coordinates = columnInfo.CoordinateInfos.Select(this.CopyFieldInfo),
        //        Data = columnInfo.DataInfos.Select(this.CopyFieldInfo)
        //    };
        //}

        public async Task<RecordInfo> GetRecordInfo(CostElementInfo[] costElementInfos)
        {
            var coordinateMetas = this.GetCoordinateMetas(costElementInfos);
            var dependencyItems = await this.GetDependencyItems(costElementInfos);
            var queryInfo = this.BuildQueryInfo(costElementInfos, coordinateMetas, dependencyItems);

            var dataInfos = 
                queryInfo.DataInfos.SelectMany(
                    costBlockInfo => costBlockInfo.CostElementInfos.Select(costElementInfo => costBlockInfo.BuildDataInfo(costElementInfo)));

            return new RecordInfo
            {
                Coordinates = coordinateMetas.Select(meta => meta.Name).ToArray(),
                Data = dataInfos.ToArray()
            };
        }

        public async Task<IDictionary<string, IEnumerable<NamedId>>> GetDependencyItems(CostElementInfo[] costElementInfos)
        {
            var result = new Dictionary<string, IEnumerable<NamedId>>();

            foreach (var costElementInfo in costElementInfos)
            {
                foreach (var costElementId in costElementInfo.CostElementIds)
                {
                    var dependecyField = costElementInfo.Meta.GetDomainDependencyField(costElementId);
                    if (dependecyField != null)
                    {
                        var items = await this.sqlRepository.GetNameIdItems(dependecyField.ReferenceMeta, dependecyField.ReferenceValueField, dependecyField.ReferenceFaceField);

                        result.Add(dependecyField.ReferenceMeta.Name, items);
                    }
                }
            }

            return result;
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

        private DataInfo CopyFieldInfo(DataInfo fieldInfo)
        {
            return new DataInfo
            {
                DataIndex = fieldInfo.DataIndex,
                CostElementId = fieldInfo.CostElementId,
                CostBlockId = fieldInfo.CostBlockId,
                ApplicationId = fieldInfo.ApplicationId
            };
        }

        //private UnionSqlHelper BuildGetRecordsQuery(
        //    CostElementInfo[] costElementInfos, 
        //    QueryInfo columnInfo, 
        //    IEnumerable<CoordinateFieldInfo> coordinateFieldInfos)
        //{
        //    var coordinateDictionary = coordinateFieldInfos.ToDictionary(info => info.CoordinateField.Name);

        //    var costBlockQueryInfos = costElementInfos.Select(info => new
        //    {
        //        FromMeta = info.Meta,
        //        SelectColumns = info.CostElementIds.SelectMany(costElementId => new[] 
        //        {
        //            SqlFunctions.Max(costElementId, info.Meta.Name, costElementId) as BaseColumnInfo,
        //            SqlFunctions.Count(
        //                costElementId, 
        //                true,
        //                info.Meta.Name, 
        //                this.BuildCountColumnAlias(info.Meta, costElementId)) as BaseColumnInfo
        //        }),
        //        GroupByColumns = info.Meta.CoordinateFields.Where(field => coordinateDictionary.ContainsKey(field.Name))
        //                                                   .Select(field => new ColumnInfo(field.Name, info.Meta.Name))
        //                                                   .ToArray()
        //    });

        //    var costBlockQueries = costBlockQueryInfos.Select(info => new
        //    {
        //        Meta = info.FromMeta,
        //        Query = Sql.Select(info.SelectColumns.Concat(info.GroupByColumns).ToArray())
        //                   .From(info.FromMeta)
        //                   .WhereNotDeleted(info.FromMeta)
        //                   .GroupBy(info.GroupByColumns)
        //                   .ToSqlBuilder()
        //    }).ToArray();

        //    var columns =
        //        columnInfo.CoordinateInfos.SelectMany(coordinate => new[] { coordinate.Id, coordinate.Name })
        //                                  .Concat(columnInfo.CostBlockInfos.SelectMany(data => new[] { data.Value, data.Count }))
        //                                  .ToArray();

        //    var firstQuery = costBlockQueries[0];
        //    var joinQuery = Sql.Select(columns).FromQuery(firstQuery.Query, firstQuery.Meta.Name);
        //    var joinedCostBlocks = new List<CostBlockEntityMeta> { firstQuery.Meta };

        //    for (var index = 1; index < costBlockQueries.Length; index++)
        //    {
        //        var costBlockQuery = costBlockQueries[index];
        //        var conditions = new List<ConditionHelper>();

        //        foreach (var coordinateField in costBlockQuery.Meta.CoordinateFields)
        //        {
        //            var conditionMeta = joinedCostBlocks.FirstOrDefault(
        //                joinedCostBlock => 
        //                    coordinateDictionary.ContainsKey(coordinateField.Name) && 
        //                    joinedCostBlock.ContainsCoordinateField(coordinateField.Name));

        //            if (conditionMeta != null)
        //            {
        //                conditions.Add(SqlOperators.Equals(
        //                    new ColumnInfo(coordinateField.Name, conditionMeta.Name),
        //                    new ColumnInfo(coordinateField.Name, costBlockQuery.Meta.Name)));
        //            }
        //        }

        //        var query = new AliasSqlBuilder
        //        {
        //            Alias = costBlockQuery.Meta.Name,
        //            Query = new BracketsSqlBuilder
        //            {
        //                Query = costBlockQuery.Query
        //            }
        //        };

        //        joinQuery = joinQuery.Join(query, ConditionHelper.And(conditions));

        //        joinedCostBlocks.Add(costBlockQuery.Meta);
        //    }

        //    var joinInfos = coordinateDictionary.Values.Select(info => new JoinInfo(info.Meta, info.CoordinateField.Name));
        //    var orderByColumns = columnInfo.CoordinateInfos.Select(info => info.Name).ToArray();

        //    return joinQuery.Join(joinInfos).OrderBy(SortDirection.Asc, orderByColumns);
        //}

        private UnionSqlHelper BuildGetRecordsQuery(QueryInfo queryInfo)
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
                    GroupByColumns = queryInfo.CoordinateInfos.Select(info => new ColumnInfo(info.CoordinateMeta.Name, costBlockInfo.Alias)).ToArray()
                };
            });

            var costBlockQueries = costBlockQueryInfos.Select(info => new
            {
                info.From,
                Query = Sql.Select(info.SelectColumns.Concat(info.GroupByColumns).ToArray())
                           .From(info.From.Meta, info.From.Alias)
                           .WhereNotDeleted(info.From.Meta, info.Filter, info.From.Alias)
                           .GroupBy(info.GroupByColumns)
                           .ToSqlBuilder()
            }).ToArray();

            var coordinateIdColumns = new List<ColumnInfo>();
            var coordinateNameColumns = new List<ColumnInfo>();

            foreach (var info in queryInfo.CoordinateInfos)
            {
                coordinateIdColumns.Add(new ColumnInfo(info.CoordinateMeta.IdField.Name, info.CoordinateMeta.Name, info.IdColumn));
                coordinateNameColumns.Add(new ColumnInfo(info.CoordinateMeta.NameField.Name, info.CoordinateMeta.Name, info.NameColumn));
            }

            var dataColumns = queryInfo.DataInfos.SelectMany(costBlockInfo => costBlockInfo.CostElementInfos.SelectMany(costElementInfo => new[] 
            {
                new ColumnInfo(costElementInfo.ValueColumn, costBlockInfo.Alias),
                new ColumnInfo(costElementInfo.CountColumn, costBlockInfo.Alias)
            }));

            var columns = coordinateIdColumns.Concat(coordinateNameColumns).Concat(dataColumns).ToArray();

            var firstQuery = costBlockQueries[0];
            var joinQuery = Sql.Select(columns).FromQuery(firstQuery.Query, firstQuery.From.Alias);

            for (var index = 1; index < costBlockQueries.Length; index++)
            {
                var costBlockQuery = costBlockQueries[index];

                var conditions = 
                    queryInfo.CoordinateInfos.Select(
                        info => SqlOperators.Equals(
                            new ColumnInfo(info.CoordinateMeta.Name, firstQuery.From.Alias),
                            new ColumnInfo(info.CoordinateMeta.Name, costBlockQuery.From.Alias)));

                var query = new AliasSqlBuilder
                {
                    Alias = costBlockQuery.From.Alias,
                    Query = new BracketsSqlBuilder
                    {
                        Query = costBlockQuery.Query
                    }
                };

                joinQuery = joinQuery.Join(query, ConditionHelper.And(conditions));
            }

            var joinInfos = queryInfo.CoordinateInfos.Select(info => new JoinInfo(firstQuery.From.Meta, info.CoordinateMeta.Name, null, firstQuery.From.Alias));

            return joinQuery.Join(joinInfos).OrderBy(SortDirection.Asc, coordinateNameColumns.ToArray());
        }

        //private string BuildColumnAlias(BaseEntityMeta meta, string costElementId, long? dependencyItemId = null)
        //{
        //    return this.SerializeDataId(meta.Name, costElementId, dependencyItemId);
        //}

        //private string BuildCountColumnAlias(BaseEntityMeta meta, string costElementId, long? dependencyItemId = null)
        //{
        //    return this.BuildColumnAlias(meta, $"{costElementId}_Count", dependencyItemId);
        //}

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
            IDictionary<string, IEnumerable<NamedId>> dependencyItems)
        {
            ////var result = new QueryInfo();

            ////foreach (var info in coordinateFieldInfos)
            ////{
            ////    var coordinateColumnInfo = new CoordinateColumnInfo
            ////    {
            ////        //MetaId = info.Meta.Name,
            ////        //FieldName = info.CoordinateField.Name,
            ////        //DataIndex = info.CoordinateField.Name,
            ////        Id = new ColumnInfo(info.CoordinateField.ReferenceValueField, info.CoordinateField.ReferenceMeta.Name, info.CoordinateField.Name),
            ////        Name = new ColumnInfo(info.CoordinateField.ReferenceFaceField, info.CoordinateField.ReferenceMeta.Name, $"{info.CoordinateField.Name}_Face")
            ////    };

            ////    result.CoordinateInfos.Add(coordinateColumnInfo);
            ////}

            ////////foreach (var costBlockInfo in costBlockInfos)
            ////////{
            ////////    foreach (var costElementId in costBlockInfo.CostElementIds)
            ////////    {
            ////////        var countAlias = this.BuildCountColumnAlias(costBlockInfo.Meta, costElementId);
            ////////        var dataIndex = this.BuildColumnAlias(costBlockInfo.Meta, costElementId);
            ////////        var dataColumnInfo = new DataColumnInfo
            ////////        {
            ////////            SchemaId = costBlockInfo.Meta.Schema,
            ////////            MetaId = costBlockInfo.Meta.Name,
            ////////            FieldName = costElementId,
            ////////            DataIndex = dataIndex,
            ////////            Value = new ColumnInfo(costElementId, costBlockInfo.Meta.Name, dataIndex),
            ////////            Count = new ColumnInfo(countAlias, costBlockInfo.Meta.Name, countAlias)
            ////////        };

            ////////        result.DataInfos.Add(dataColumnInfo);
            ////////    }
            ////////}

            //////foreach (var costElementInfo in costElementInfos)
            //////{
            //////    var costBlockQueryInfo = new CostBlockQueryInfo
            //////    {
            //////        Meta = costElementInfo.Meta,
            //////        CostElementInfos = new List<BaseCostElementQueryInfo>()
            //////    };

            //////    foreach (var costElementId in costElementInfo.CostElementIds)
            //////    {
            //////        BaseCostElementQueryInfo costElementQueryInfo;

            //////        var dependencyField = costElementInfo.Meta.GetDomainDependencyField(costElementId);
            //////        if (dependencyField == null)
            //////        {
            //////            costElementQueryInfo = new CostElementQueryInfo
            //////            {
            //////                CostElementId = costElementId,
            //////                DataColumnInfo = BuildDataColumnInfo(costElementInfo.Meta, costElementId, costElementId)
            //////            };
            //////        }
            //////        else
            //////        {
            //////            costElementQueryInfo = new CostElementDependencyQueryInfo
            //////            {
            //////                CostElementId = costElementId,
            //////                DependencyField = dependencyField,
            //////                DependencyItemInfos = 
            //////                    dependencyItems[dependencyField.ReferenceMeta.Name].Select(item => new DependencyItemQueryInfo
            //////                    {
            //////                        DependencyItemId = item.Id,
            //////                        DataColumnInfo = BuildDataColumnInfo(costElementInfo.Meta, dependencyField.ReferenceMeta.Name, costElementId, item.Id)
            //////                    }).ToArray()
            //////            };
            //////        }

            //////        costBlockQueryInfo.CostElementInfos.Add(costElementQueryInfo);
            //////    }

            //////    result.DataInfos.Add(costBlockQueryInfo);
            //////}

            ////foreach (var costElementInfo in costElementInfos)
            ////{
            ////    var dependencyGroups =
            ////        costElementInfo.CostElementIds.Select(costElementId => new { CostElementId = costElementId, DependencyField = costElementInfo.Meta.GetDomainDependencyField(costElementId) })
            ////                                      .GroupBy(info => info.DependencyField?.ReferenceMeta);

            ////    foreach(var dependencyGroup in dependencyGroups)
            ////    {
            ////        IEnumerable<BaseCostElementQueryInfo> costElementQueryInfos;
            ////        string alias;

            ////        if (dependencyGroup.Key == null)
            ////        {
            ////            alias = costElementInfo.Meta.Name;

            ////            costElementQueryInfos = dependencyGroup.Select(info => new CostElementQueryInfo
            ////            {
            ////                CostElementId = info.CostElementId,
            ////                DataColumnInfo = BuildDataColumnInfo(costElementInfo.Meta, info.CostElementId, info.CostElementId)
            ////            });
            ////        }
            ////        else
            ////        {
            ////            alias = $"{costElementInfo.Meta.Name}_{dependencyGroup.Key.Name}";

            ////            costElementQueryInfos = dependencyGroup.Select(info => new CostElementDependencyQueryInfo
            ////            {
            ////                CostElementId = info.CostElementId,
            ////                DependencyField = info.DependencyField,
            ////                DependencyItemInfos =
            ////                    dependencyItems[info.DependencyField.ReferenceMeta.Name].Select(item => new DependencyItemQueryInfo
            ////                    {
            ////                        DependencyItemId = item.Id,
            ////                        DataColumnInfo = BuildDataColumnInfo(costElementInfo.Meta, info.DependencyField.ReferenceMeta.Name, info.CostElementId, item.Id)
            ////                    }).ToArray()
            ////            });
            ////        }

            ////        result.DataInfos.Add(new CostBlockQueryInfo
            ////        {
            ////            Alias = alias,
            ////            Meta = costElementInfo.Meta,
            ////            CostElementInfos = costElementQueryInfos.ToArray()
            ////        });
            ////    }
            ////}

            //var result = new QueryInfo
            //{
            //    CoordinateFields = new HashSet<string>(this.GetCoordinateFieldNames(costElementInfos)),
            //    DataInfos = new List<CostBlockQueryInfo>()
            //};

            //foreach (var costElementInfo in costElementInfos)
            //{
            //    var dependencyGroups =
            //        costElementInfo.CostElementIds.Select(costElementId => new { CostElementId = costElementId, DependencyField = costElementInfo.Meta.GetDomainDependencyField(costElementId) })
            //                                      .GroupBy(info => info.DependencyField?.ReferenceMeta);

            //    foreach (var dependencyGroup in dependencyGroups)
            //    {
            //        if (dependencyGroup.Key == null)
            //        {
            //            result.DataInfos.Add(new CostBlockQueryInfo
            //            {
            //                TableAlias = costElementInfo.Meta.Name,
            //                Meta = costElementInfo.Meta,
            //                CostElementInfos = 
            //                    dependencyGroup.Select(info => BuildDataColumnInfo(costElementInfo.Meta, info.CostElementId, info.CostElementId))
            //                                   .ToArray()
            //            });
            //        }
            //        else
            //        {
            //            foreach(var info in dependencyGroup)
            //            {
            //                foreach(var item in dependencyItems[info.DependencyField.ReferenceMeta.Name])
            //                {
            //                    var alias = $"{costElementInfo.Meta.Name}_{dependencyGroup.Key.Name}_{item.Id}";

            //                    result.DataInfos.Add(new CostBlockQueryInfo
            //                    {
            //                        TableAlias = alias,
            //                        Meta = costElementInfo.Meta,
            //                        WhereCondition = SqlOperators.Equals(info.CostElementId, "dependencyItemId", item.Id, alias),
            //                        DependencyItemId = item.Id,
            //                        CostElementInfos = new[]
            //                        {
            //                            BuildDataColumnInfo(costElementInfo.Meta, info.DependencyField.ReferenceMeta.Name, info.CostElementId, item.Id)
            //                        }
            //                    });
            //                }
            //            }
            //        }
            //    }
            //}

            //return result;

            //DataColumnInfo BuildDataColumnInfo(CostBlockEntityMeta meta, string costElementId, long? dependendyItemId = null)
            //{
            //    var countAlias = this.BuildCountColumnAlias(meta, costElementId, dependendyItemId);
            //    var dataIndex = this.BuildColumnAlias(meta, costElementId, dependendyItemId);

            //    return new DataColumnInfo
            //    {
            //        Value = new ColumnInfo(costElementId, meta.Name, dataIndex),
            //        Count = new ColumnInfo(countAlias, meta.Name, countAlias)
            //    };
            //}

            var result = new QueryInfo
            {
                DataInfos = new List<CostBlockQueryInfo>(),
                CoordinateInfos = coordinateMetas.Select(meta => new CoordinateColumnInfo
                {
                    CoordinateMeta = meta,
                    IdColumn = $"{meta.Name}_Id",
                    NameColumn = $"{meta.Name}_Name"

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
                        foreach (var item in dependencyItems[dependencyGroup.Key.Name])
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

            //T BuildCostBlockQueryInfo<T>(CostBlockEntityMeta meta, IEnumerable<string> costElementIds, string namePostfix) where T : CostBlockQueryInfo, new()
            //{
            //    var costElInfos = costElementIds.Select(costElementId => new DataColumnInfo
            //    {
            //        ValueColumn = $"{meta.Name}_{costElementId}_value",
            //        CountColumn = $"{meta.Name}_{costElementId}_count"
            //    });

            //    return new T
            //    {
            //        Meta = meta,
            //        CostElementInfos =
            //            dependencyGroup.Select(costElementId => BuildDataColumnInfo(costElementInfo.Meta, costElementId))
            //                            .ToArray()
            //    };
            //}
        }

        //private IEnumerable<CoordinateFieldInfo> GetCoordinateFieldInfos(CostElementInfo[] costBlockInfos)
        //{
        //    var coordinateLists = costBlockInfos.Select(info => info.Meta.CoordinateFields.Select(field => field.Name)).ToArray();

        //    var fieldNames = coordinateLists[0];

        //    for (var index = 1; index < coordinateLists.Length; index++)
        //    {
        //        fieldNames = fieldNames.Intersect(coordinateLists[index]);
        //    }

        //    foreach (var fieldName in fieldNames)
        //    {
        //        ReferenceFieldMeta coordinateField;

        //        foreach (var costBlockInfo in costBlockInfos)
        //        {
        //            coordinateField = (ReferenceFieldMeta)costBlockInfo.Meta.GetField(fieldName);

        //            if (coordinateField != null)
        //            {
        //                yield return new CoordinateFieldInfo
        //                {
        //                    Meta = costBlockInfo.Meta,
        //                    CoordinateField = coordinateField
        //                };

        //                break;
        //            }
        //        }
        //    }
        //}

        private IEnumerable<NamedEntityMeta> GetCoordinateMetas(CostElementInfo[] costElementInfos)
        {
            var coordinateLists = 
                costElementInfos.Select(info => info.Meta.InputLevelFields.Select(field => field.ReferenceMeta).OfType<NamedEntityMeta>())
                                .ToArray();

            var metas = coordinateLists[0];

            for (var index = 1; index < coordinateLists.Length; index++)
            {
                metas = metas.Intersect(coordinateLists[index]);
            }

            return metas;
        }

        //private class CoordinateFieldInfo
        //{
        //    public CostBlockEntityMeta Meta { get; set; }

        //    public ReferenceFieldMeta CoordinateField { get; set; }
        //}

        //private class CoordinateColumnInfo : FieldInfo
        //{
        //    public ColumnInfo Id { get; set; }

        //    public ColumnInfo Name { get; set; }
        //}

        //private class DataColumnInfo : DataInfo
        //{
        //    public ColumnInfo Value { get; set; }

        //    public ColumnInfo Count { get; set; }
        //}

        private class CoordinateColumnInfo 
        {
            public NamedEntityMeta CoordinateMeta { get; set; }

            public string IdColumn { get; set; }

            public string NameColumn { get; set; }
        }

        private class DataColumnInfo
        {
            public string CostElementId { get; set; }

            public string DataIndex { get; set; }

            public string ValueColumn { get; set; }

            public string CountColumn { get; set; }
        }

        //////private class BaseCostElementQueryInfo
        //////{
        //////    public string CostElementId { get; set; }
        //////}

        //////private class CostElementQueryInfo : BaseCostElementQueryInfo
        //////{
        //////    public DataColumnInfo DataColumnInfo { get; set; }
        //////}

        //////private class DependencyItemQueryInfo
        //////{
        //////    public long DependencyItemId { get; set; }

        //////    public DataColumnInfo DataColumnInfo { get; set; }
        //////}

        //////private class CostElementDependencyQueryInfo : BaseCostElementQueryInfo
        //////{
        //////    public ReferenceFieldMeta DependencyField { get; set; }

        //////    public IEnumerable<DependencyItemQueryInfo> DependencyItemInfos { get; set; }
        //////}

        ////private class CostElementQueryInfo
        ////{
        ////    public string CostElementId { get; set; }

        ////    public DataColumnInfo DataColumnInfo { get; set; }
        ////}

        //private class BaseCostBlockQueryInfo
        //{
        //    public CostBlockEntityMeta Meta { get; set; }
        //}

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
                return SerializeDataIndex(this.Meta.ApplicationId, dataColumnInfo.CostElementId);
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
                return SerializeDataIndex(this.Meta.ApplicationId, dataColumnInfo.CostElementId, this.DependencyItemId);
            }
        }

        //private class CostBlockQueryInfo
        //{
        //    public CostBlockEntityMeta Meta { get; set; }

        //    public IEnumerable<DataColumnInfo> CostElementInfos { get; set; }

        //    public BaseEntityMeta DependencyMeta { get; set; }
        //}


        //private class CostBlockQueryInfo
        //{
        //    public CostBlockEntityMeta Meta { get; set; }

        //    public string TableAlias { get; set; }

        //    //public IEnumerable<BaseCostElementQueryInfo> CostElementInfos { get; set; }

        //    public ConditionHelper WhereCondition { get; set; }

        //    public long? DependencyItemId { get; set; }

        //    //public IEnumerable<CostElementQueryInfo> CostElementInfos { get; set; }

        //    public IEnumerable<DataColumnInfo> CostElementInfos { get; set; }
        //}

        //private class CostElementDataInfo
        //{
        //    public IDictionary<string, DependencyItemInfo> CostElements { get; set; }
        //}

        //private class TableViewColumnInfo
        //{
        //    public List<CoordinateColumnInfo> CoordinateInfos { get; private set; }

        //    public List<DataColumnInfo> DataInfos { get; private set; }

        //    public TableViewColumnInfo()
        //    {
        //        this.CoordinateInfos = new List<CoordinateColumnInfo>();
        //        this.DataInfos = new List<DataColumnInfo>();
        //    }
        //}

        private class QueryInfo
        {
            public List<CoordinateColumnInfo> CoordinateInfos { get; set; }

            //public HashSet<string> CoordinateFields { get; set; }

            public List<CostBlockQueryInfo> DataInfos { get; set; }

            //public QueryInfo()
            //{
            //    //this.CoordinateInfos = new List<CoordinateColumnInfo>();
            //    this.DataInfos = new List<CostBlockQueryInfo>();
            //}
        }
    }
}
