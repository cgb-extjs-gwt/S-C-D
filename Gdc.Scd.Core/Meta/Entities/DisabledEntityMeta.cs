using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DisabledEntityMeta : NamedEntityMeta, IDisabledEntityMeta
    {
        public SimpleFieldMeta IsDisabledField { get; }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                foreach(var field in base.AllFields)
                {
                    yield return field;
                }

                yield return this.IsDisabledField;
            }
        }

        public DisabledEntityMeta(string name, string shema) 
            : base(name, shema)
        {
            this.IsDisabledField = new SimpleFieldMeta(nameof(BaseDisabledEntity.IsDisabled), TypeCode.Boolean);
        }
    }
}
