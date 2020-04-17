using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockEntityMeta : BaseCostBlockEntityMeta
    {
        public CostBlockValueHistoryEntityMeta HistoryMeta { get; set; }

        public IDictionary<FieldMeta, FieldMeta> CostElementsApprovedFields { get; } = new Dictionary<FieldMeta, FieldMeta>();

        public IEnumerable<FieldMeta> AllCostElemetFields
        {
            get
            {
                return this.CostElementsFields.Concat(this.CostElementsApprovedFields.Values);
            }
        }

        public CreatedDateTimeFieldMeta CreatedDateField { get; } = new CreatedDateTimeFieldMeta();

        public SimpleFieldMeta DeletedDateField { get; } = new SimpleFieldMeta(nameof(IDeactivatable.DeactivatedDateTime), TypeCode.DateTime) { IsNullOption = true };

        public MetaCollection<FieldMeta> AdditionalFields { get; } = new MetaCollection<FieldMeta>();

        public ReferenceFieldMeta ActualVersionField { get; }

        public CostBlockMeta SliceDomainMeta { get; }

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                var fields = 
                    base.AllFields.Concat(this.CostElementsApprovedFields.Values)
                                  .Concat(this.AdditionalFields);

                foreach (var field in fields)
                {
                    yield return field;
                }

                yield return this.CreatedDateField;
                yield return this.DeletedDateField;
                yield return this.ActualVersionField;
            }
        }

        public CostBlockEntityMeta(CostBlockMeta sliceMeta, string name, string shema)
            : base(name, shema)
        {
            this.SliceDomainMeta = sliceMeta;
            this.ActualVersionField = new ReferenceFieldMeta("ActualVersion", this, this.IdField.Name)
            {
                IsNullOption = true
            };
        }

        public FieldMeta GetApprovedCostElement(string costElementId)
        {
            var costElementField = this.CostElementsFields[costElementId];

            this.CostElementsApprovedFields.TryGetValue(costElementField, out var approvedCostElement);

            return approvedCostElement;
        }

        public IEnumerable<ReferenceFieldMeta> GetDomainInputLevelFields(string costElementId)
        {
            return this.GetDomainInputLevelFields(this.SliceDomainMeta.CostElements[costElementId]);
        }

        public ReferenceFieldMeta GetDomainDependencyField(string costElementId)
        {
            ReferenceFieldMeta dependencyField = null;

            var costElement = this.SliceDomainMeta.CostElements[costElementId];
            if (costElement.Dependency != null)
            {
                dependencyField = this.DependencyFields[costElement.Dependency.Id];
            }

            return dependencyField;
        }

        public IEnumerable<ReferenceFieldMeta> GetDomainCoordinateFields(string costElementId)
        {
            var costElement = this.SliceDomainMeta.CostElements[costElementId];
            
            foreach (var inputLevelField in this.GetDomainInputLevelFields(costElement))
            {
                yield return inputLevelField;
            }

            if (costElement.Dependency != null)
            {
                yield return this.DependencyFields[costElement.Dependency.Id];
            }
        }

        public QualityGate GetQualityGate(string costElementId)
        {
            return this.SliceDomainMeta.CostElements[costElementId].QualityGate;
        }

        public ReferenceFieldMeta GetDomainCoordinateField(string costElementId, string fieldName)
        {
            return
                this.GetDomainCoordinateFields(costElementId)
                    .FirstOrDefault(field => field.Name == fieldName);
        }

        public ReferenceFieldMeta GetDomainRegionInputField(string costElementId)
        {
            ReferenceFieldMeta regionField = null;

            var regionInput = this.SliceDomainMeta.CostElements[costElementId].RegionInput;
            if (regionInput != null)
            {
                regionField = this.InputLevelFields[regionInput.Id];
            }

            return regionField;
        }

        private IEnumerable<ReferenceFieldMeta> GetDomainInputLevelFields(CostElementMeta costElement)
        {
            foreach(var field in costElement.InputLevels.Select(inputLevel => this.InputLevelFields[inputLevel.Id]))
            {
                yield return field;
            }

            if (costElement.RegionInput != null && !costElement.HasInputLevel(costElement.RegionInput.Id))
            {
                yield return this.InputLevelFields[costElement.RegionInput.Id];
            }
        }
    }
}
