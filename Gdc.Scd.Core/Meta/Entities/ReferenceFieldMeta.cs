using System;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class ReferenceFieldMeta : FieldMeta
    {
        public EntityMeta ReferenceMeta { get; }

        public string ReferenceValueField { get; set; }

        public string ReferenceFaceField { get; set; }

        public SimpleFieldMeta ForeignField { get; }

        public ReferenceFieldMeta(string name, EntityMeta referenceMeta, SimpleFieldMeta foreignField) 
            : base(name)
        {
            this.ReferenceMeta = referenceMeta;
            this.ForeignField = foreignField;
        }

        public ReferenceFieldMeta(string name, EntityMeta referenceMeta)
            : this(name, referenceMeta, new SimpleFieldMeta(name, TypeCode.Int64))
        {
        }
    }
}
