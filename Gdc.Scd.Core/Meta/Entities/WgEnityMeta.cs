using System;
using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class WgEnityMeta : BaseWgSogEntityMeta
    {
        public ReferenceFieldMeta SogField { get; }
        public ReferenceFieldMeta CompanyField { get; }
        public ReferenceFieldMeta CentralContractGroupField { get; }

        public ReferenceFieldMeta RoleCodeField { get; }

        public SimpleFieldMeta WgTypeField { get; }

        public SimpleFieldMeta DescriptionField { get; }

        public SimpleFieldMeta ResponsiblePersonField { get; }

	    public WgEnityMeta(NamedEntityMeta plaMeta, 
            NamedEntityMeta sfabMeta, 
            NamedEntityMeta sogMeta, 
            NamedEntityMeta centralContractGroupMeta, 
            NamedEntityMeta roleCode, NamedEntityMeta companyMeta) 
            : base(MetaConstants.WgInputLevelName, MetaConstants.InputLevelSchema, plaMeta, sfabMeta)
        {
            this.SogField = ReferenceFieldMeta.Build(nameof(Wg.SogId), sogMeta);
            this.WgTypeField = new SimpleFieldMeta(nameof(Wg.WgType), TypeCode.Int32);
            this.CentralContractGroupField = ReferenceFieldMeta.Build(nameof(Wg.CentralContractGroupId), centralContractGroupMeta);
            this.DescriptionField = new SimpleFieldMeta(nameof(Wg.Description), TypeCode.String);
            this.ResponsiblePersonField = new SimpleFieldMeta(nameof(Wg.ResponsiblePerson), TypeCode.String);
            this.RoleCodeField = ReferenceFieldMeta.Build(nameof(Wg.RoleCodeId), roleCode, true);
            this.CompanyField = ReferenceFieldMeta.Build(nameof(Wg.CompanyId), companyMeta);
        }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.SogField;
                yield return this.CentralContractGroupField;
                yield return this.WgTypeField;
                yield return this.DescriptionField;
                yield return this.ResponsiblePersonField;
                yield return this.RoleCodeField;
                yield return this.CompanyField;

                foreach (var field in base.AllFields)
                {
                    yield return field;
                }
            }
        }
    }
}
