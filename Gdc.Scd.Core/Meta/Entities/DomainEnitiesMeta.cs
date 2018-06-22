using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainEnitiesMeta
    {
        public MetaCollection<EntityMeta> CostBlocks { get; private set; } = new MetaCollection<EntityMeta>();

        public MetaCollection<EntityMeta> Dependencies { get; private set; } = new MetaCollection<EntityMeta>();

        public MetaCollection<EntityMeta> InputLevels { get; private set; } = new MetaCollection<EntityMeta>();
    }
}
