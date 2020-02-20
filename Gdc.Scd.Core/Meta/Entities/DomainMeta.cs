using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class DomainMeta : BaseDomainMeta<CostBlockMeta, ApplicationMeta>
    {
        public CostElementMeta GetCostElement(ICostElementIdentifier costElementIdentifier)
        {
            return this.GetCostElement(costElementIdentifier.CostBlockId, costElementIdentifier.CostElementId);
        }

        public CostElementMeta GetCostElement(string costBlockId, string costElementId)
        {
            return this.CostBlocks[costBlockId].CostElements[costElementId];
        }
    }
}
