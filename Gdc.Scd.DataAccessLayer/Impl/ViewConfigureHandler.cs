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

        public bool IsAlterView { get; set; }

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

                CombinedViewInfo.Build<DurationAvailability>(
                    "DurationAvailability",
                    ReferenceInfo.Build<Duration>(nameof(DurationAvailability.Year)),
                    ReferenceInfo.Build<Availability>(nameof(DurationAvailability.Availability)))
            };
        }

        void IConfigureDatabaseHandler.Handle()
        {
            Action<CombinedViewInfo> action;

            if (this.IsAlterView)
            {
                action = viewInfo => this.CreateCombinedView<AlterViewSqlBuilder>(viewInfo);
            }
            else
            {
                action = viewInfo => this.CreateCombinedView<CreateViewSqlBuilder>(viewInfo);
            }

            foreach (var viewInfo in this.combinedViewInfos)
            {
                action(viewInfo);
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
            var combinedMetas = new List<NamedEntityMeta>();
            var referenceMetas = new Dictionary<string, NamedEntityMeta>();

            foreach (var viewInfo in this.combinedViewInfos)
            {
                var meta = this.IsDisabledEntity(viewInfo)
                    ? new DisabledEntityMeta(viewInfo.ViewName, viewInfo.Shema)
                    : new NamedEntityMeta(viewInfo.ViewName, viewInfo.Shema);

                foreach (var referenceInfo in viewInfo.ReferenceInfos)
                {
                    var referenceName = BaseEntityMeta.BuildFullName(referenceInfo.ReferenceEntityInfo.Name, referenceInfo.ReferenceEntityInfo.Schema);

                    if (!referenceMetas.TryGetValue(referenceName, out var referenceMeta))
                    {
                        referenceMeta = this.BuildNamedMeta(referenceInfo.ReferenceEntityInfo);
                        referenceMetas.Add(referenceName, referenceMeta);
                    }

                    meta.Fields.Add(ReferenceFieldMeta.Build(referenceInfo.ForeignColumn, referenceMeta));
                }

                combinedMetas.Add(meta);
            }

            return combinedMetas.Concat(referenceMetas.Values);
        }

        private void CreateCombinedView<T>(CombinedViewInfo viewInfo)
            where T : BaseViewSqlBuilder, new()
        {
            var combinedEntityInfo = MetaHelper.GetEntityInfo(viewInfo.CombinedType);
            var combineEntity = new EntityMeta(combinedEntityInfo.Name, combinedEntityInfo.Schema);

            combineEntity.Fields.Add(new IdFieldMeta());

            var columnWithSpaces = new List<ISqlBuilder>();

            ISqlBuilder prevColumn = null;

            for (var index = 0; index < viewInfo.ReferenceInfos.Length; index++)
            {
                var referenceInfo = viewInfo.ReferenceInfos[index];
                var meta = this.BuildNamedMeta(referenceInfo.ReferenceEntityInfo);

                combineEntity.Fields.Add(ReferenceFieldMeta.Build(referenceInfo.ForeignColumn, meta));

                var column = new ColumnSqlBuilder
                {
                    Name = meta.NameField.Name,
                    Table = meta.Name
                };

                columnWithSpaces.Add(
                    index == 0
                        ? (ISqlBuilder)column
                        : new BracketsSqlBuilder
                        {
                            Query = new CaseSqlBuilder
                            {
                                Cases = new[]
                                {
                                    new CaseItem
                                    {
                                        When = 
                                            SqlOperators.Equals(prevColumn, new StringValueSqlBuilder(MetaConstants.NoneValue))
                                                        .And(SqlOperators.Equals(column, new StringValueSqlBuilder(MetaConstants.NoneValue)))
                                                        .ToSqlBuilder(),
                                        Then = new StringValueSqlBuilder("")
                                    }
                                },
                                Else = SqlOperators.Add(new StringValueSqlBuilder(" "), column).ToSqlBuilder()
                            }
                        });

                prevColumn = column;
            }

            var idColumn = new ColumnInfo(IdFieldMeta.DefaultId, combineEntity.Name, IdFieldMeta.DefaultId);
            var nameColumn = new QueryColumnInfo(
                    SqlOperators.Add(columnWithSpaces.ToArray()).ToSqlBuilder(),
                    MetaConstants.NameFieldKey);

            var innerColumns = new List<BaseColumnInfo>{ idColumn, nameColumn };

            innerColumns.AddRange(
                viewInfo.ReferenceInfos.Select(
                    info => new ColumnInfo(info.ForeignColumn, combineEntity.Name, info.ForeignColumn)));

            if (this.IsDisabledEntity(viewInfo))
            {
                innerColumns.Add(new ColumnInfo(nameof(BaseDisabledEntity.IsDisabled), combineEntity.Name, nameof(BaseDisabledEntity.IsDisabled)));
            }

            var query = Sql.Select(innerColumns.ToArray()).From(combineEntity);

            foreach (var referenceInfo in viewInfo.ReferenceInfos)
            {
                query = query.Join(combineEntity, referenceInfo.ForeignColumn);
            }

            var createViewQuery = new T
            {
                Shema = MetaConstants.DependencySchema,
                Name = viewInfo.ViewName,
                Query = query.ToSqlBuilder()
            };

            this.repositorySet.ExecuteSql(new SqlHelper(createViewQuery));
        }

        private bool IsDisabledEntity(CombinedViewInfo viewInfo)
        {
            return typeof(BaseDisabledEntity).IsAssignableFrom(viewInfo.CombinedType);
        }

        private NamedEntityMeta BuildNamedMeta(EntityInfo info)
        {
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

            public EntityInfo ReferenceEntityInfo { get; private set; }

            public static ReferenceInfo Build<T>(string property) where T : NamedId
            {
                var type = typeof(T);

                return new ReferenceInfo
                {
                    Type = type,
                    ForeignColumn = $"{property}Id",
                    ReferenceEntityInfo = MetaHelper.GetEntityInfo(type)
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
