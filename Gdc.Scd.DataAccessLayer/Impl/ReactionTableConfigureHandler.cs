using System;
using System.Collections.Generic;
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

        private const string ReactionTimeKey = "ReactionTime";

        private const string ReactionTypeKey = "ReactionType";

        private const string AvailabilityKey = "Availability";

        private const string ReactionAvailabilityKey = "ReactionTimeAvalability";

        private readonly IServiceProvider serviceProvider;

        private bool isReactionTimeCreated;

        public ReactionTableConfigureHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanHandle(BaseEntityMeta entityMeta)
        {
            return
                entityMeta.Schema == MetaConstants.DependencySchema && 
                (entityMeta.Name == ReactionKey || entityMeta.Name == AvailabilityKey);
        }

        public IEnumerable<ISqlBuilder> GetSqlBuilders(BaseEntityMeta entityMeta)
        {
            var reactionTimeEntity = new NamedEntityMeta(ReactionTimeKey, MetaConstants.DependencySchema);
            var sqlBuilders = new List<ISqlBuilder>();

            if (!this.isReactionTimeCreated)
            {
                sqlBuilders.Add(new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = reactionTimeEntity });

                this.isReactionTimeCreated = true;
            }

            if (entityMeta.Name == ReactionKey)
            {
                var reactionTypeEntity = new NamedEntityMeta(ReactionTypeKey, MetaConstants.DependencySchema);

                sqlBuilders.Add(new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = reactionTypeEntity });
                sqlBuilders.AddRange(this.BuildCombinedView(reactionTimeEntity, reactionTypeEntity, ReactionKey));
            }
            else if (entityMeta.Name == AvailabilityKey)
            {
                sqlBuilders.Add(new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = entityMeta });
                sqlBuilders.AddRange(this.BuildCombinedView(reactionTimeEntity, (NamedEntityMeta)entityMeta, ReactionAvailabilityKey));
            }

            return sqlBuilders;
        }

        private IEnumerable<ISqlBuilder> BuildCombinedView(NamedEntityMeta meta1, NamedEntityMeta meta2, string viewName)
        {
            var combineEntity = new EntityMeta($"{meta1.Name}_{meta2.Name}", MetaConstants.DependencySchema);

            combineEntity.Fields.Add(new IdFieldMeta());
            combineEntity.Fields.Add(ReferenceFieldMeta.Build(meta1.Name, meta1));
            combineEntity.Fields.Add(ReferenceFieldMeta.Build(meta2.Name, meta2));

            yield return new CreateTableMetaSqlBuilder(this.serviceProvider) { Meta = combineEntity };

            yield return new CreateViewSqlBuilder
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
                       .Join(combineEntity, meta1.Name)
                       .Join(combineEntity, meta2.Name)
                       .ToSqlBuilder()
            };
        }
    }
}
