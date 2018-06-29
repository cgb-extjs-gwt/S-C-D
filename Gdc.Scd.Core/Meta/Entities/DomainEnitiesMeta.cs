using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public BaseEntityMeta this[string fullName] => (EntityMeta)this.CostBlocks[fullName] ?? this.Dependencies[fullName] ?? this.InputLevels[fullName];

        public MetaCollection<CostBlockEntityMeta> CostBlocks { get; private set; } = new MetaCollection<CostBlockEntityMeta>();

        public MetaCollection<NamedEntityMeta> Dependencies { get; private set; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; private set; } = new MetaCollection<NamedEntityMeta>();

        public NamedEntityMeta CountryInputLevel => this.InputLevels[MetaConstants.CountryLevelId];

        public IEnumerable<BaseEntityMeta> AllMetas => this.CostBlocks.Cast<EntityMeta>().Concat(this.Dependencies).Concat(this.InputLevels);

        public BaseEntityMeta GetEntityMeta(string name, string schema = null)
        {
            var fullName = BaseEntityMeta.BuildFullName(name, schema);

            return this[fullName];
        }
    }
}
