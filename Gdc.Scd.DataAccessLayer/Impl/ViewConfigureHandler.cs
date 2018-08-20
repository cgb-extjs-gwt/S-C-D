using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
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
        private const string ReactionTimeTypeKey = "ReactionTimeType";

        private const string ReactionTimeAvailabilityKey = "ReactionTimeAvailability";

        private readonly IRepositorySet repositorySet;

        public ViewConfigureHandler(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        void IConfigureDatabaseHandler.Handle()
        {
            this.CreateCombinedView<ReactionTime, ReactionType, ReactionTimeType>(
                ReactionTimeTypeKey, 
                $"{nameof(ReactionTimeType.ReactionTime)}Id", 
                $"{nameof(ReactionTimeType.ReactionType)}Id");

            this.CreateCombinedView<ReactionTime, Availability, ReactionTimeAvalability>(
                ReactionTimeAvailabilityKey,
                $"{nameof(ReactionTimeAvalability.ReactionTime)}Id",
                $"{nameof(ReactionTimeAvalability.Availability)}Id");
        }

        IEnumerable<ISqlBuilder> ICustomConfigureTableHandler.GetSqlBuilders(BaseEntityMeta entityMeta)
        {
            if (entityMeta is CostBlockEntityMeta)
            {
                if (entityMeta.GetField(ReactionTimeTypeKey) != null)
                {
                    yield return this.BuildConstraint<ReactionTimeType>(entityMeta, ReactionTimeTypeKey);
                }

                if (entityMeta.GetField(ReactionTimeAvailabilityKey) != null)
                {
                    yield return this.BuildConstraint<ReactionTimeAvalability>(entityMeta, ReactionTimeAvailabilityKey);
                }
            }
        }

        private void CreateCombinedView<T1, T2, TCombined>(string viewName, string foreignColumn1, string foreignColumn2) 
            where T1 : NamedId
            where T2 : NamedId
        {
            var meta1 = this.BuildNamedMeta<T1>();
            var meta2 = this.BuildNamedMeta<T2>();
            var combinedEntityInfo = this.GetEntityInfo<TCombined>();
            var combineEntity = new EntityMeta(combinedEntityInfo.Table, combinedEntityInfo.Schema);

            combineEntity.Fields.Add(new IdFieldMeta());
            combineEntity.Fields.Add(ReferenceFieldMeta.Build(foreignColumn1, meta1));
            combineEntity.Fields.Add(ReferenceFieldMeta.Build(foreignColumn2, meta2));

            var query = new CreateViewSqlBuilder
            {
                Shema = MetaConstants.DependencySchema,
                Name = viewName,
                Query =
                    Sql.Select(
                        new ColumnInfo(IdFieldMeta.DefaultId, combineEntity.Name, IdFieldMeta.DefaultId),
                        new QueryColumnInfo(
                            new RawSqlBuilder
                            {
                                RawSql = $"([{meta1.Name}].[{meta1.NameField.Name}] + ' ' + [{meta2.Name}].[{meta2.NameField.Name}])"
                            },
                            MetaConstants.NameFieldKey))
                       .From(combineEntity)
                       .Join(combineEntity, foreignColumn1)
                       .Join(combineEntity, foreignColumn2)
                       .ToSqlBuilder()
            };

            this.repositorySet.ExecuteSql(new SqlHelper(query));
        }

        private NamedEntityMeta BuildNamedMeta<T>() where T : NamedId
        {
            var info = this.GetEntityInfo<T>();

            return new NamedEntityMeta(info.Table, info.Schema);
        }

        private (string Table, string Schema) GetEntityInfo<T>()
        {
            string table;
            string schema;

            var type = typeof(T);
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

        private ISqlBuilder BuildConstraint<T>(BaseEntityMeta entityMeta, string foreignField) where T : IIdentifiable
        {
            var entityInfo = this.GetEntityInfo<T>();
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
    }
}
