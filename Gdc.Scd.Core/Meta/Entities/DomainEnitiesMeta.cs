using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public BaseEntityMeta this[string fullName] => (BaseEntityMeta)this.CostBlocks[fullName] ?? this.Dependencies[fullName] ?? this.InputLevels[fullName];

        public MetaCollection<CostBlockEntityMeta> CostBlocks { get; } = new MetaCollection<CostBlockEntityMeta>();

        public MetaCollection<NamedEntityMeta> Dependencies { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; } = new MetaCollection<NamedEntityMeta>();

        public IEnumerable<BaseEntityMeta> AllMetas => this.CostBlocks.Cast<BaseEntityMeta>().Concat(this.Dependencies).Concat(this.InputLevels);

        public BaseEntityMeta GetEntityMeta(string name, string schema = null)
        {
            var fullName = BaseEntityMeta.BuildFullName(name, schema);

            return this[fullName];
        }
    }
}
