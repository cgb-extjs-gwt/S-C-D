using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockEntityMeta : BaseEntityMeta
    {
        public IdFieldMeta IdField { get; } = new IdFieldMeta();

        public MetaCollection<ReferenceFieldMeta> InputLevelFields { get; } = new MetaCollection<ReferenceFieldMeta>();

        public MetaCollection<FieldMeta> DependencyFields { get; } = new MetaCollection<FieldMeta>();

        public MetaCollection<FieldMeta> CostElementsFields { get; } = new MetaCollection<FieldMeta>();

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.IdField;

                var fields = this.InputLevelFields.Concat(this.DependencyFields).Concat(this.CostElementsFields);

                foreach (var field in fields)
                {
                    yield return field;
                }
            }
        }

        public CostBlockEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }
    }
}
