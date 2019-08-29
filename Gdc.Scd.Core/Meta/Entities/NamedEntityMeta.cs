using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Constants;

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

                foreach (var field in base.AllFields)
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

        public NamedEntityMeta(string name, string shema = null, string nameField = null)
            : this(name, new SimpleFieldMeta(nameField ?? MetaConstants.NameFieldKey, TypeCode.String), shema)
        {
        }
    }
}
