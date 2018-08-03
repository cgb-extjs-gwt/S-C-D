using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockEntityMeta : BaseCostBlockEntityMeta
    {
        public CostBlockValueHistoryEntityMeta HistoryMeta { get; set; }

        public IDictionary<FieldMeta, FieldMeta> CostElementsApprovedFields { get; } = new Dictionary<FieldMeta, FieldMeta>();

        public CreatedDateTimeFieldMeta CreatedDateField { get; set; } = new CreatedDateTimeFieldMeta();

        public SimpleFieldMeta DeletedDateField { get; set; } = new SimpleFieldMeta("DeletedDateTime", TypeCode.DateTime) { IsNullOption = true };

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                var fields = base.AllFields.Concat(this.CostElementsApprovedFields.Values);

                foreach (var field in fields)
                {
                    yield return field;
                }

                yield return this.CreatedDateField;
                yield return this.DeletedDateField;
            }
        }

        public CostBlockEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }
    }
}
