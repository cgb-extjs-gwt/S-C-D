namespace Gdc.Scd.Core.Meta.Entities
{
    public class ReferenceFieldMeta : FieldMeta
    {
        public EntityMeta ReferenceMeta { get; }

        public string ValueField { get; set; }

        public string FaceValueField { get; set; }

        public ReferenceFieldMeta(string name, EntityMeta referenceMeta) 
            : base(name)
        {
            this.ReferenceMeta = referenceMeta;
        }
    }
}
