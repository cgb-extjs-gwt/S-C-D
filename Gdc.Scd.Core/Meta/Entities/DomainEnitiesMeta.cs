using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public BaseEntityMeta this[string fullName]
        {
            get
            {
                return this.CostBlocks[fullName] ?? this.Dependencies[fullName] ?? this.InputLevels[fullName] ?? this.OtherMetas[fullName];
            }
        }

        public MetaCollection<CostBlockEntityMeta> CostBlocks { get; } = new MetaCollection<CostBlockEntityMeta>();

        public MetaCollection<NamedEntityMeta> Dependencies { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<NamedEntityMeta> InputLevels { get; } = new MetaCollection<NamedEntityMeta>();

        public MetaCollection<BaseEntityMeta> OtherMetas { get; } = new MetaCollection<BaseEntityMeta>();

        public IEnumerable<BaseEntityMeta> AllMetas
        {
            get
            {
                return 
                    this.CostBlocks.Cast<BaseEntityMeta>()
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
    }
}
