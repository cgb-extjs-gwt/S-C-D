using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockValueHistoryEntityMeta : BaseCostBlockEntityMeta
    {
        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                foreach (var field in base.AllFields)
                {
                    yield return field;
                }

                yield return this.CostBlockHistoryField;
            }
        }

        public ReferenceFieldMeta CostBlockHistoryField { get; set; }

        public MetaCollection<RelatedItemsHistoryEntityMeta> RelatedMetas { get; } = new MetaCollection<RelatedItemsHistoryEntityMeta>();

        public CostBlockValueHistoryEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }

        public RelatedItemsHistoryEntityMeta GetRelatedMetaByName(string name)
        {
            var fullName = BuildFullName(name, MetaConstants.HistoryRelatedItemsSchema);

            return this.RelatedMetas[fullName];
        }
    }
}
