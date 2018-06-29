using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public EntityMeta this[string fullName] => this.CostBlocks[fullName] ?? this.Dependencies[fullName] ?? this.InputLevels[fullName];

        public MetaCollection<EntityMeta> CostBlocks { get; private set; } = new MetaCollection<EntityMeta>();

        public MetaCollection<EntityMeta> Dependencies { get; private set; } = new MetaCollection<EntityMeta>();

        public MetaCollection<EntityMeta> InputLevels { get; private set; } = new MetaCollection<EntityMeta>();

        public IEnumerable<EntityMeta> AllMetas => this.CostBlocks.Concat(this.Dependencies).Concat(this.InputLevels);

        public EntityMeta GetEntityMeta(string name, string schema = null)
        {
            var fullName = EntityMeta.BuildFullName(name, schema);

            return this[fullName];
        }
    }
}
