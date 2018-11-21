using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class SFabEntityMeta : DeactivatableEntityMeta
    {
        public ReferenceFieldMeta PlaField { get; }

        public SFabEntityMeta(NamedEntityMeta plaMeta) 
            : base(MetaConstants.SfabInputLevel, MetaConstants.InputLevelSchema)
        {
            this.PlaField = ReferenceFieldMeta.Build(nameof(SFab.PlaId), plaMeta);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.PlaField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
