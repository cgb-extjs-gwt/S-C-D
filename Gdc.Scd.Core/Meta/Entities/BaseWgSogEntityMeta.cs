using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class BaseWgSogEntityMeta : DeactivatableEntityMeta
    {
        public ReferenceFieldMeta PlaField { get; }

        public ReferenceFieldMeta SFabField { get; }

        public SimpleFieldMeta IsSoftwareField { get; }

        public BaseWgSogEntityMeta(string name, string shema, NamedEntityMeta plaMeta, NamedEntityMeta sfabMeta) 
            : base(name, shema)
        {
            this.PlaField = ReferenceFieldMeta.Build(nameof(BaseWgSog.PlaId), plaMeta);
            this.SFabField = ReferenceFieldMeta.Build(nameof(BaseWgSog.SFabId), sfabMeta);
            this.IsSoftwareField = new SimpleFieldMeta(nameof(BaseWgSog.IsSoftware), TypeCode.Boolean);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.PlaField;
                yield return this.SFabField;
                yield return this.IsSoftwareField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
