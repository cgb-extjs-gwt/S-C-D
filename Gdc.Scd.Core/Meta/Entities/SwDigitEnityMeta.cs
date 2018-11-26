using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class SwDigitEnityMeta : NamedEntityMeta
    {
        public ReferenceFieldMeta SogField { get; }

        public SwDigitEnityMeta(NamedEntityMeta sogMeta) 
            : base(MetaConstants.SwDigitInputLevel, MetaConstants.InputLevelSchema)
        {
            this.SogField = ReferenceFieldMeta.Build(nameof(SwDigit.SogId), sogMeta);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.SogField;              
            }
        }
    }
}
