using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CountryEntityMeta : NamedEntityMeta
    {
        public SimpleFieldMeta QualityGateGroupField { get; } 

        public ReferenceFieldMeta ClusterRegionField { get; }

        public SimpleFieldMeta IsMasterField { get; }

        public ReferenceFieldMeta CurrencyField { get; }

        public CountryEntityMeta(string name, string schema, ClusterRegionEntityMeta clusterRegionMeta, NamedEntityMeta currencyMeta)
            : base(name, schema)
        {
            this.QualityGateGroupField = new SimpleFieldMeta(nameof(Country.QualityGateGroup), TypeCode.String);
            this.ClusterRegionField = ReferenceFieldMeta.Build(nameof(Country.ClusterRegionId), clusterRegionMeta);
            this.IsMasterField = new SimpleFieldMeta(nameof(Country.IsMaster), TypeCode.Boolean);
            this.CurrencyField = ReferenceFieldMeta.Build(nameof(Country.CurrencyId), currencyMeta);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.QualityGateGroupField;
                yield return this.ClusterRegionField;
                yield return this.IsMasterField;
                yield return this.CurrencyField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
