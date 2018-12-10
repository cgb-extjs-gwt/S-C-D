using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class SwDigitEnityMeta : DeactivatableEntityMeta
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

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
