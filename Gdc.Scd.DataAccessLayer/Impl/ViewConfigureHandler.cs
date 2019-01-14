using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Helpers;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ViewConfigureHandler : IConfigureDatabaseHandler, ICustomConfigureTableHandler, ICoordinateEntityMetaProvider
    {
        private readonly IRepositorySet repositorySet;

        private readonly CombinedViewInfo[] combinedViewInfos;

        public ViewConfigureHandler(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;

            this.combinedViewInfos = new CombinedViewInfo[]
            {
                CombinedViewInfo.Build<ReactionTimeType>(
                    "ReactionTimeType",
                    ReferenceInfo.Build<ReactionTime>(nameof(ReactionTimeType.ReactionTime)),
                    ReferenceInfo.Build<ReactionType>(nameof(ReactionTimeType.ReactionType))),

                CombinedViewInfo.Build<ReactionTimeAvalability>(
                    "ReactionTimeAvailability",
                    ReferenceInfo.Build<ReactionTime>(nameof(ReactionTimeAvalability.ReactionTime)),
                    ReferenceInfo.Build<Availability>(nameof(ReactionTimeAvalability.Availability))),

                CombinedViewInfo.Build<ReactionTimeTypeAvalability>(
                    "ReactionTimeTypeAvailability",
                    ReferenceInfo.Build<ReactionTime>(nameof(ReactionTimeTypeAvalability.ReactionTime)),
                    ReferenceInfo.Build<Availability>(nameof(ReactionTimeTypeAvalability.Availability)),
                    ReferenceInfo.Build<ReactionType>(nameof(ReactionTimeTypeAvalability.ReactionType))),

                CombinedViewInfo.Build<YearAvailability>(
                    "YearAvailability",
                    ReferenceInfo.Build<Year>(nameof(YearAvailability.Year)),
                    ReferenceInfo.Build<Availability>(nameof(YearAvailability.Availability)))
            };
        }

        void IConfigureDatabaseHandler.Handle()
        {
            foreach (var viewInfo in this.combinedViewInfos)
            {
                this.CreateCombinedView(viewInfo);
            }
        }

        IEnumerable<ISqlBuilder> ICustomConfigureTableHandler.GetSqlBuilders(BaseEntityMeta entityMeta)
        {
            if (entityMeta is CostBlockEntityMeta)
            {
                foreach (var viewInfo in this.combinedViewInfos)
                {
                    if (entityMeta.GetField(viewInfo.ViewName) != null)
                    {
                        yield return this.BuildConstraint(viewInfo.CombinedType, entityMeta, viewInfo.ViewName);
                    }
                }
            }
        }

        IEnumerable<NamedEntityMeta> ICoordinateEntityMetaProvider.GetCoordinateEntityMetas()
        {
            return
                this.combinedViewInfos.Where(this.IsDisabledEntity)
                                      .Select(info => new DisabledEntityMeta(info.ViewName, info.Shema));
        }

        private void CreateCombinedView(CombinedViewInfo viewInfo)
        {
            var combinedEntityInfo = MetaHelper.GetEntityInfo(viewInfo.CombinedType);
            var combineEntity = new EntityMeta(combinedEntityInfo.Name, combinedEntityInfo.Schema);

            combineEntity.Fields.Add(new IdFieldMeta());

            var columnWithSpaces = new List<ISqlBuilder>();
            var lastIndex = viewInfo.ReferenceInfos.Length - 1;

            for (var index = 0; index < viewInfo.ReferenceInfos.Length; index++)
            {
                var referenceInfo = viewInfo.ReferenceInfos[index];
                var meta = this.BuildNamedMeta(referenceInfo.Type);

                combineEntity.Fields.Add(ReferenceFieldMeta.Build(referenceInfo.ForeignColumn, meta));

                var column = new ColumnSqlBuilder
                {
                    Name = meta.NameField.Name,
                    Table = meta.Name
                };

                columnWithSpaces.Add(new BracketsSqlBuilder
                {
                    Query = new CaseSqlBuilder
                    {
                        Input = column,
                        Cases = new []
                        {
                            new CaseItem
                            {
                                When = new StringValueSqlBuilder(MetaConstants.NoneValue),
                                Then = new StringValueSqlBuilder("")
                            }
                        },
                        Else = index < lastIndex 
                            ? SqlOperators.Add(column, new StringValueSqlBuilder(" ")).ToSqlBuilder() 
                            : column
                    }
                });
            }

            var idColumn = new ColumnInfo(IdFieldMeta.DefaultId, combineEntity.Name, IdFieldMeta.DefaultId);
            var nameColumn = new QueryColumnInfo(
                    SqlOperators.Add(columnWithSpaces.ToArray()).ToSqlBuilder(),
                    MetaConstants.NameFieldKey);

            var innerColumns = new List<BaseColumnInfo>{ idColumn, nameColumn };

            if (this.IsDisabledEntity(viewInfo))
            {
                innerColumns.Add(new ColumnInfo(nameof(BaseDisabledEntity.IsDisabled), combineEntity.Name, nameof(BaseDisabledEntity.IsDisabled)));
            }

            var query = Sql.Select(innerColumns.ToArray()).From(combineEntity);

            foreach (var referenceInfo in viewInfo.ReferenceInfos)
            {
                query = query.Join(combineEntity, referenceInfo.ForeignColumn);
            }

            var columns = innerColumns.Select(
                column =>
                    column == nameColumn
                        ? new QueryColumnInfo
                        {
                            Alias = nameColumn.Alias,
                            Query = new CaseSqlBuilder
                            {
                                Input = new ColumnSqlBuilder(nameColumn.Alias),
                                Cases = new[]
                                {
                                    new CaseItem
                                    {
                                        When = new StringValueSqlBuilder(""),
                                        Then = new StringValueSqlBuilder(MetaConstants.NoneValue)
                                    }
                                },
                                Else = new ColumnSqlBuilder(nameColumn.Alias)
                            }
                        }
                        : new ColumnInfo(column.Alias) as BaseColumnInfo);

            var createViewQuery = new CreateViewSqlBuilder
            {
                Shema = MetaConstants.DependencySchema,
                Name = viewInfo.ViewName,
                Query = Sql.Select(columns.ToArray()).FromQuery(query, "t").ToSqlBuilder()
            };

            this.repositorySet.ExecuteSql(new SqlHelper(createViewQuery));
        }

        private bool IsDisabledEntity(CombinedViewInfo viewInfo)
        {
            return typeof(BaseDisabledEntity).IsAssignableFrom(viewInfo.CombinedType);
        }

        private NamedEntityMeta BuildNamedMeta(Type type)
        {
            var info = MetaHelper.GetEntityInfo(type);

            return new NamedEntityMeta(info.Name, info.Schema);
        }

        private ISqlBuilder BuildConstraint(Type type, BaseEntityMeta entityMeta, string foreignField)
        {
            var entityInfo = MetaHelper.GetEntityInfo(type);
            var refMeta = new EntityMeta(entityInfo.Name, entityInfo.Schema);
            refMeta.Fields.Add(new IdFieldMeta());

            var constraintMeta = new EntityMeta(entityMeta.Name, entityMeta.Schema);
            constraintMeta.Fields.Add(new ReferenceFieldMeta(foreignField, refMeta, IdFieldMeta.DefaultId));

            return new CreateColumnConstraintMetaSqlBuilder
            {
                Meta = constraintMeta,
                Field = foreignField
            };
        }

        private class ReferenceInfo
        {
            public Type Type { get; private set; }

            public string ForeignColumn { get; private set; }

            public static ReferenceInfo Build<T>(string property) where T : NamedId
            {
                return new ReferenceInfo
                {
                    Type = typeof(T),
                    ForeignColumn = $"{property}Id"
                };
            }
        }

        private class CombinedViewInfo
        {
            public Type CombinedType { get; private set; }

            public ReferenceInfo[] ReferenceInfos { get; private set; }

            public string ViewName { get; private set; }

            public string Shema { get; private set; }

            public static CombinedViewInfo Build<TCombined>(string viewName, params ReferenceInfo[] referenceInfos)
            {
                var combinedType = typeof(TCombined);
                var entityInfo = MetaHelper.GetEntityInfo(combinedType);

                return new CombinedViewInfo
                {
                    CombinedType = combinedType,
                    ViewName = viewName,
                    Shema = entityInfo.Schema,
                    ReferenceInfos = referenceInfos
                };
            }
        }
    }
}
