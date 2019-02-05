using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class ClusterRegionEntityMeta : NamedEntityMeta
    {
        public SimpleFieldMeta IsEmeiaField { get; private set; }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                foreach(var field in base.AllFields)
                {
                    yield return field;
                }

                yield return this.IsEmeiaField;
            }
        }

        public ClusterRegionEntityMeta()
            : base(MetaConstants.ClusterRegionInputLevel, MetaConstants.InputLevelSchema)
        {
            this.IsEmeiaField = new SimpleFieldMeta("IsEmeia", TypeCode.Boolean);
        }
    }
}
