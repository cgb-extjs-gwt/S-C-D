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

            var nameColumns = new List<string>();

            foreach (var referenceInfo in viewInfo.ReferenceInfos)
            {
                var meta = this.BuildNamedMeta(referenceInfo.Type);

                combineEntity.Fields.Add(ReferenceFieldMeta.Build(referenceInfo.ForeignColumn, meta));

                var columnSqlBuilder = new ColumnSqlBuilder
                {
                    Name = meta.NameField.Name,
                    Table = meta.Name
                };

                nameColumns.Add(columnSqlBuilder.Build(null));
            }

            var columns = new List<BaseColumnInfo>
            {
                new ColumnInfo(IdFieldMeta.DefaultId, combineEntity.Name, IdFieldMeta.DefaultId),
                new QueryColumnInfo(
                    new RawSqlBuilder
                    {
                        RawSql = $"({string.Join(" + ' ' + ", nameColumns)})"
                    },
                    MetaConstants.NameFieldKey)
            };

            if (this.IsDisabledEntity(viewInfo))
            {
                columns.Add(new ColumnInfo(nameof(BaseDisabledEntity.IsDisabled), combineEntity.Name));
            }

            var query = Sql.Select(columns.ToArray()).From(combineEntity);

            foreach (var referenceInfo in viewInfo.ReferenceInfos)
            {
                query = query.Join(combineEntity, referenceInfo.ForeignColumn);
            }

            var createViewQuery = new CreateViewSqlBuilder
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
