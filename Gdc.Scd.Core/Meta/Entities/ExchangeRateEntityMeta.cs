using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class ExchangeRateEntityMeta : BaseEntityMeta
    {
        public ReferenceFieldMeta CurrencyField { get; }

        public SimpleFieldMeta Value { get; }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.CurrencyField;
                yield return this.Value;
            }
        }

        public ExchangeRateEntityMeta(NamedEntityMeta currencyMeta) 
            : base(MetaConstants.ExchangeRateTable, MetaConstants.ReferencesSchema)
        {
            this.CurrencyField = ReferenceFieldMeta.Build(ExchangeRate.CurrencyIdColumn, currencyMeta);
            this.Value = new SimpleFieldMeta(nameof(ExchangeRate.Value), TypeCode.Double);
        }
    }
}
