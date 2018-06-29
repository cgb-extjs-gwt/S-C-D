using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class NamedEntityMeta : EntityMeta
    {
        public IdFieldMeta IdField { get; } = new IdFieldMeta();

        public SimpleFieldMeta NameField { get; }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.IdField;
                yield return this.NameField;

                foreach (var field in this.Fields)
                {
                    yield return field;
                }
            }
        }

        public NamedEntityMeta(string name, SimpleFieldMeta nameField, string shema = null) 
            : base(name, shema)
        {
            this.NameField = nameField;
        }
    }
}
