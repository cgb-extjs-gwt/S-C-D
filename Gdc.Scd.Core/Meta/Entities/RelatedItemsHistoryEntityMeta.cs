using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class RelatedItemsHistoryEntityMeta : BaseEntityMeta
    {
        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.CostBlockHistoryField;
                yield return this.RelatedItemField;
            }
        }

        public ReferenceFieldMeta CostBlockHistoryField { get; set; }

        public ReferenceFieldMeta RelatedItemField { get; set; }

        public RelatedItemsHistoryEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }
    }
}
