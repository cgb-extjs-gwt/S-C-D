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

        public CostBlockMeta DomainMeta { get; }

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

        public CostBlockEntityMeta(CostBlockMeta meta, string name, string shema = null)
            : base(name, shema)
        {
            this.DomainMeta = meta;
        }

        public FieldMeta GetApprovedCostElement(string costElementId)
        {
            var costElementField = this.CostElementsFields[costElementId];

            this.CostElementsApprovedFields.TryGetValue(costElementField, out var approvedCostElement);

            return approvedCostElement;
        }

        public IEnumerable<ReferenceFieldMeta> GetDomainInputLevelFields(string costElementId)
        {
            return this.GetDomainInputLevelFields(this.DomainMeta.CostElements[costElementId]);
        }

        public ReferenceFieldMeta GetDomainDependencyField(string costElementId)
        {
            ReferenceFieldMeta dependencyField = null;

            var costElement = this.DomainMeta.CostElements[costElementId];
            if (costElement.Dependency != null)
            {
                dependencyField = this.DependencyFields[costElement.Dependency.Id];
            }

            return dependencyField;
        }

        public IEnumerable<ReferenceFieldMeta> GetDomainCoordinateFields(string costElementId)
        {
            var costElement = this.DomainMeta.CostElements[costElementId];
            
            foreach (var inputLevelField in this.GetDomainInputLevelFields(costElement))
            {
                yield return inputLevelField;
            }

            if (costElement.Dependency != null)
            {
                yield return this.DependencyFields[costElement.Dependency.Id];
            }
        }

        private IEnumerable<ReferenceFieldMeta> GetDomainInputLevelFields(CostElementMeta costElement)
        {
            return costElement.InputLevels.Select(inputLevel => this.InputLevelFields[inputLevel.Id]);
        }
    }
}
