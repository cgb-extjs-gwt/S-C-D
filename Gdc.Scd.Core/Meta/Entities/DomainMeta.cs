using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainMeta : BaseDomainMeta<CostBlockMeta, ApplicationMeta>
    {
        public CostElementMeta GetCostElement(ICostElementIdentifier costElementIdentifier)
        {
            return this.CostBlocks[costElementIdentifier.CostBlockId].CostElements[costElementIdentifier.CostElementId];
        }
    }
}
