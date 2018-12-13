using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DeactivatableEntityMeta : NamedEntityMeta
    {
        public SimpleFieldMeta CreatedDateTimeField { get; }

        public SimpleFieldMeta DeactivatedDateTimeField { get; }

        public SimpleFieldMeta ModifiedDateTimeField { get; }

        public DeactivatableEntityMeta(string name, string shema) 
            : base(name, shema)
        {
            this.CreatedDateTimeField = new SimpleFieldMeta(nameof(IDeactivatable.CreatedDateTime), TypeCode.DateTime);
            this.DeactivatedDateTimeField = new SimpleFieldMeta(nameof(IDeactivatable.DeactivatedDateTime), TypeCode.DateTime);
            this.ModifiedDateTimeField = new SimpleFieldMeta(nameof(IDeactivatable.ModifiedDateTime), TypeCode.DateTime);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.CreatedDateTimeField;
                yield return this.DeactivatedDateTimeField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
