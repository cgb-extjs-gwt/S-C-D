using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainMeta
    {
        public MetaCollection<CostBlockMeta> CostBlocks { get; set; }

        public MetaCollection<ApplicationMeta> Applications { get; set; }

        public CostElementMeta GetCostElement(ICostElementIdentifier costElementIdentifier)
        {
            return this.CostBlocks[costElementIdentifier.CostBlockId].CostElements[costElementIdentifier.CostElementId];
        }
    }
}
