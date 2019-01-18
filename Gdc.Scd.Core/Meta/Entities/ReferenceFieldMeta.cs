namespace Gdc.Scd.Core.Meta.Entities
{
    public class ReferenceFieldMeta : FieldMeta
    {
        public BaseEntityMeta ReferenceMeta { get; }

        public string ReferenceValueField { get; private set; }

        public string ReferenceFaceField { get; set; }

        public ReferenceFieldMeta(string name, BaseEntityMeta referenceMeta, string referenceValueField)
            : base(name)
        {
            this.ReferenceMeta = referenceMeta;
            this.ReferenceValueField = referenceValueField;
        }

        public static ReferenceFieldMeta Build(string name,  NamedEntityMeta referenceMeta, bool isNullable = false)
        {
            return new ReferenceFieldMeta(name, referenceMeta, referenceMeta.IdField.Name)
            {
                ReferenceFaceField = referenceMeta.NameField.Name,
                IsNullOption = isNullable
            };
        }
    }
}
