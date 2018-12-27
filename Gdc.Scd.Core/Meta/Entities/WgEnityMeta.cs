using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class WgEnityMeta : BaseWgSogEntityMeta
    {
        public ReferenceFieldMeta SogField { get; }

        public SimpleFieldMeta WgTypeField { get; }

        public SimpleFieldMeta DescriptionField { get; }

        public SimpleFieldMeta ResponsiblePersonField { get; }

        public WgEnityMeta(NamedEntityMeta plaMeta, NamedEntityMeta sfabMeta, NamedEntityMeta sogMeta) 
            : base(MetaConstants.WgInputLevelName, MetaConstants.InputLevelSchema, plaMeta, sfabMeta)
        {
            this.SogField = ReferenceFieldMeta.Build(nameof(Wg.SogId), sogMeta);
            this.WgTypeField = new SimpleFieldMeta(nameof(Wg.WgType), TypeCode.Int32);
            this.DescriptionField = new SimpleFieldMeta(nameof(Wg.Description), TypeCode.String);
            this.ResponsiblePersonField = new SimpleFieldMeta(nameof(Wg.ResponsiblePerson), TypeCode.String);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.SogField;
                yield return this.WgTypeField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
