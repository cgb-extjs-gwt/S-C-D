using System;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class ReferenceFieldMeta : FieldMeta
    {
        public BaseEntityMeta ReferenceMeta { get; }

        public string ReferenceValueField { get; set; }

        public string ReferenceFaceField { get; set; }

        public SimpleFieldMeta ForeignField { get; }

        public ReferenceFieldMeta(string name, BaseEntityMeta referenceMeta, SimpleFieldMeta foreignField) 
            : base(name)
        {
            this.ReferenceMeta = referenceMeta;
            this.ForeignField = foreignField;
        }

        public ReferenceFieldMeta(string name, BaseEntityMeta referenceMeta)
            : this(name, referenceMeta, new SimpleFieldMeta(name, TypeCode.Int64))
        {
        }

        public static ReferenceFieldMeta Build(string name,  NamedEntityMeta referenceMeta)
        {
            return new ReferenceFieldMeta(name, referenceMeta)
            {
                ReferenceValueField = referenceMeta.IdField.Name,
                ReferenceFaceField = referenceMeta.NameField.Name,
            };
        }
    }
}
