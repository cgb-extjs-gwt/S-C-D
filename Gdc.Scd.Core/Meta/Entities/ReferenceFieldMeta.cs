namespace Gdc.Scd.Core.Meta.Entities
{
    public class ReferenceFieldMeta : FieldMeta
    {
        public string RefEnitityFullName { get; set; }

        public string ValueField { get; set; }

        public string FaceValueField { get; set; }

        public ReferenceFieldMeta(string id) 
            : base(id)
        {
        }
    }
}
