using System;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DeactivatableEntityMeta : NamedEntityMeta
    {
        public SimpleFieldMeta CreatedDateTimeField { get; }

        public SimpleFieldMeta DeactivatedDateTimeField { get; }

        public SimpleFieldMeta ModifiedDateTimeField { get; }

        public DeactivatableEntityMeta(string name, string shema = null) 
            : base(name, new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String), shema)
        {
            this.CreatedDateTimeField = new SimpleFieldMeta(nameof(IDeactivatable.CreatedDateTime), TypeCode.DateTime);
            this.DeactivatedDateTimeField = new SimpleFieldMeta(nameof(IDeactivatable.DeactivatedDateTime), TypeCode.DateTime);
            this.ModifiedDateTimeField = new SimpleFieldMeta(nameof(IDeactivatable.ModifiedDateTime), TypeCode.DateTime);
        }
    }
}
