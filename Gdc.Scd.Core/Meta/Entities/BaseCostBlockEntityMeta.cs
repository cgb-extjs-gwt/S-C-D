using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public abstract class BaseCostBlockEntityMeta : BaseEntityMeta, ICostBlockIdentifier
    {
        public IdFieldMeta IdField { get; } = new IdFieldMeta();

        public MetaCollection<ReferenceFieldMeta> InputLevelFields { get; } = new MetaCollection<ReferenceFieldMeta>();

        public MetaCollection<ReferenceFieldMeta> DependencyFields { get; } = new MetaCollection<ReferenceFieldMeta>();

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

        string ICostBlockIdentifier.ApplicationId => this.Schema;

        string ICostBlockIdentifier.CostBlockId => this.Name;

        public BaseCostBlockEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }
    }
}
