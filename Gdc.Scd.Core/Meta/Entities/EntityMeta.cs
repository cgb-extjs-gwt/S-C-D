using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class EntityMeta : BaseEntityMeta
    {
        public MetaCollection<FieldMeta> Fields { get; private set; } = new MetaCollection<FieldMeta>();

        public override IEnumerable<FieldMeta> AllFields => this.Fields;

        public EntityMeta(string name, string shema) 
            : base(name, shema)
        {
        }

        public EntityMeta(string name, string shema, IEnumerable<FieldMeta> fields)
            : this(name, shema)
        {
            this.Fields.AddRange(fields);
        }
    }
}
