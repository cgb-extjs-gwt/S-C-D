using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockEntityMeta : BaseCostBlockEntityMeta
    {
        //public IdFieldMeta IdField { get; } = new IdFieldMeta();

        //public MetaCollection<ReferenceFieldMeta> InputLevelFields { get; } = new MetaCollection<ReferenceFieldMeta>();

        //public MetaCollection<ReferenceFieldMeta> DependencyFields { get; } = new MetaCollection<ReferenceFieldMeta>();

        //public MetaCollection<FieldMeta> CostElementsFields { get; } = new MetaCollection<FieldMeta>();

        //public override IEnumerable<FieldMeta> AllFields
        //{
        //    get
        //    {
        //        var fields = this.InputLevelFields.Concat(this.DependencyFields).Concat(this.CostElementsFields);

        //        foreach (var field in base.AllFields)
        //        {
        //            yield return field;
        //        }
        //    }
        //}

        public CostBlockValueHistoryEntityMeta HistoryMeta { get; set; }

        public MetaCollection<FieldMeta> CostElementsApprovedFields { get; } = new MetaCollection<FieldMeta>();

        public override IEnumerable<FieldMeta> AllFields => base.AllFields.Concat(this.CostElementsApprovedFields);

        public CostBlockEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }
    }
}
