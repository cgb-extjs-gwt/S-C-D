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

        public CountryEntityMeta(string name, string schema, ClusterRegionEntityMeta clusterRegionMeta)
            : base(name, schema)
        {
            this.QualityGateGroupField = new SimpleFieldMeta(nameof(Country.QualityGateGroup), TypeCode.String);
            this.ClusterRegionField = ReferenceFieldMeta.Build(nameof(Country.ClusterRegionId), clusterRegionMeta);
            this.IsMasterField = new SimpleFieldMeta(nameof(Country.IsMaster), TypeCode.Boolean);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.QualityGateGroupField;
                yield return this.ClusterRegionField;
                yield return this.IsMasterField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
