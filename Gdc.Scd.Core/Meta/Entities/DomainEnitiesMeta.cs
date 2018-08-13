using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public BaseEntityMeta this[string fullName]
        {
            get
            {
                return
                    this.CostBlocks[fullName] ??
                    this.Dependencies[fullName] ??
                    this.InputLevels[fullName] ??
                    this.OtherMetas[fullName] ??
                    this.RelatedItemsHistories[fullName];
            }
        }

        public MetaCollection<CostBlockEntityMeta> CostBlocks { get; } = new MetaCollection<CostBlockEntityMeta>();

        public MetaCollection<NamedEntityMeta> Dependencies { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; } = new MetaCollection<NamedEntityMeta>();

        public EntityMeta CostBlockHistory { get; set; }

        public MetaCollection<BaseEntityMeta> OtherMetas { get; } = new MetaCollection<BaseEntityMeta>();

        public MetaCollection<RelatedItemsHistoryEntityMeta> RelatedItemsHistories { get; } = new MetaCollection<RelatedItemsHistoryEntityMeta>();

        public IEnumerable<BaseEntityMeta> AllMetas
        {
            get
            {
                return
                    this.CostBlocks.Cast<BaseEntityMeta>()
                                   .Concat(this.CostBlocks.Select(costBlock => costBlock.HistoryMeta))
                                   .Concat(this.RelatedItemsHistories)
                                   .Concat(this.Dependencies)
                                   .Concat(this.InputLevels)
                                   .Concat(this.OtherMetas);
            }
        }

        public BaseEntityMeta GetEntityMeta(string name, string schema = null)
        {
            var fullName = BaseEntityMeta.BuildFullName(name, schema);

            return this[fullName];
        }

        public NamedEntityMeta GetInputLevel(string name)
        {
            return (NamedEntityMeta)this.GetEntityMeta(name, MetaConstants.InputLevelSchema);
        }
    }
}
