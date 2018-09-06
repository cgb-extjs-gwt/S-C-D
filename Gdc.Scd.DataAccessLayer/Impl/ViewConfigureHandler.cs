using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl.MetaBuilders;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ViewConfigureHandler : IConfigureDatabaseHandler, ICustomConfigureTableHandler
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

        private void CreateCombinedView(CombinedViewInfo viewInfo)
        {
            var combinedEntityInfo = this.GetEntityInfo(viewInfo.CombinedType);
            var combineEntity = new EntityMeta(combinedEntityInfo.Table, combinedEntityInfo.Schema);

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

            var query =
                Sql.Select(
                    new ColumnInfo(IdFieldMeta.DefaultId, combineEntity.Name, IdFieldMeta.DefaultId),
                    new QueryColumnInfo(
                        new RawSqlBuilder
                        {
                            RawSql = $"({string.Join(" + ' ' + ", nameColumns)})"
                        },
                        MetaConstants.NameFieldKey))
                    .From(combineEntity);

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

        private NamedEntityMeta BuildNamedMeta(Type type)
        {
            var info = this.GetEntityInfo(type);

            return new NamedEntityMeta(info.Table, info.Schema);
        }

        private (string Table, string Schema) GetEntityInfo(Type type)
        {
            string table;
            string schema;

            var tableAttr = type.GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault();
            if (tableAttr == null)
            {
                table = type.Name;
                schema = MetaConstants.DefaultSchema;
            }
            else
            {
                table = tableAttr.Name;
                schema = tableAttr.Schema ?? MetaConstants.DefaultSchema;
            }

            return (Table: table, Schema: schema);
        }

        private ISqlBuilder BuildConstraint(Type type, BaseEntityMeta entityMeta, string foreignField)
        {
            var entityInfo = this.GetEntityInfo(type);
            var refMeta = new EntityMeta(entityInfo.Table, entityInfo.Schema);
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

            public static CombinedViewInfo Build<TCombined>(string viewName, params ReferenceInfo[] referenceInfos)
            {
                return new CombinedViewInfo
                {
                    CombinedType = typeof(TCombined),
                    ViewName = viewName,
                    ReferenceInfos = referenceInfos
                };
            }
        }
    }
}
