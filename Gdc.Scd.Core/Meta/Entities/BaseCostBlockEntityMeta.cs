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

        public IEnumerable<ReferenceFieldMeta> CoordinateFields => this.InputLevelFields.Concat(this.DependencyFields);

        public MetaCollection<FieldMeta> CostElementsFields { get; } = new MetaCollection<FieldMeta>();

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return this.IdField;

                var fields = this.CoordinateFields.Concat(this.CostElementsFields);

                foreach (var field in fields)
                {
                    yield return field;
                }
            }
        }

        public string ApplicationId => this.Schema;

        public string CostBlockId => this.Name;

        public BaseCostBlockEntityMeta(string name, string shema = null)
            : base(name, shema)
        {
        }

        public bool ContainsCoordinateField(string fieldName)
        {
            return this.InputLevelFields.Contains(fieldName) || this.DependencyFields.Contains(fieldName);
        }
    }
}
