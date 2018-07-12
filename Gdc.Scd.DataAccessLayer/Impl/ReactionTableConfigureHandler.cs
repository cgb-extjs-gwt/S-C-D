using System;
using System.Collections.Generic;
using System.Text;
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
    public class ReactionTableConfigureHandler : ICustomConfigureTableHandler
    {
        private const string ReactionKey = "Reaction";

        private const string NameField = "Name";

        private const string ReactionTimeKey = "ReactionTime";

        private const string ReactionTypeKey = "ReactionType";

        private readonly IServiceProvider serviceProvider;

        public ReactionTableConfigureHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanHandle(BaseEntityMeta entityMeta)
        {
            return entityMeta.Name == ReactionKey && entityMeta.Schema == MetaConstants.DependencySchema;
        }

        public IEnumerable<ISqlBuilder> GetSqlBuilders(BaseEntityMeta entityMeta)
        {
            var reactionTimeEntity = new NamedEntityMeta(ReactionTimeKey, new SimpleFieldMeta(NameField, TypeCode.String), MetaConstants.DependencySchema);
            var reactionTypeEntity = new NamedEntityMeta(ReactionTypeKey, new SimpleFieldMeta(NameField, TypeCode.String), MetaConstants.DependencySchema);
            var reactionTimeTypeTable = $"{ReactionTimeKey}_{ReactionTypeKey}";
            var reactionTimeTypeEntity = new EntityMeta(reactionTimeTypeTable, MetaConstants.DependencySchema);

            reactionTimeTypeEntity.Fields.Add(new IdFieldMeta());
            reactionTimeTypeEntity.Fields.Add(ReferenceFieldMeta.Build(ReactionTimeKey, reactionTimeEntity));
            reactionTimeTypeEntity.Fields.Add(ReferenceFieldMeta.Build(ReactionTypeKey, reactionTypeEntity));

            yield return new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = reactionTimeEntity };
            yield return new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = reactionTypeEntity };
            yield return new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = reactionTimeTypeEntity };

            yield return new CreateViewSqlBuilder
            {
                Shema = MetaConstants.DependencySchema,
                Name = ReactionKey,
                Query =
                    Sql.Select(
                        new ColumnInfo(IdFieldMeta.DefaultId, reactionTimeTypeEntity.Name, IdFieldMeta.DefaultId),
                        new QueryColumnInfo(
                            new RawSqlBuilder
                            {
                                RawSql = $"([{ReactionTimeKey}].[{NameField}] + [{ReactionTypeKey}].[{NameField}])"
                            }, 
                            NameField))
                       .From(reactionTimeTypeEntity)
                       .Join(reactionTimeTypeEntity, ReactionTimeKey)
                       .Join(reactionTimeTypeEntity, ReactionTypeKey)
                       .ToSqlBuilder()
            };
        }
    }
}
