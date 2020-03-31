using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainMeta : BaseDomainMeta<CostBlockMeta, ApplicationMeta>
    {
        public MetaCollection<InputLevelMeta> InputLevels { get; set; }

        public MetaCollection<InputLevelMeta> RegionInputs { get; set; }

        public MetaCollection<DependencyMeta> Dependencies { get; set; }

        public CostElementMeta GetCostElement(ICostElementIdentifier costElementIdentifier)
        {
            return this.CostBlocks[costElementIdentifier.CostBlockId].CostElements[costElementIdentifier.CostElementId];
        }

        public BaseMeta GetCoordinate(string coordinateId)
        {
            return
                this.Dependencies[coordinateId]
                    ?? this.InputLevels[coordinateId]
                    ?? (BaseMeta)this.RegionInputs[coordinateId];
        }
    }
}
