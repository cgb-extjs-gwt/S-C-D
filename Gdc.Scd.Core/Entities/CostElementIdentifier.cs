using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class CostElementIdentifier : ICostElementIdentifier
    {
        public string ApplicationId { get; set; }

        public string CostElementId { get; set; }

        public string CostBlockId { get; set; }

        public CostElementIdentifier()
        { 
        }

        public CostElementIdentifier(string applicationId, string costBlockId, string costElementId)
        {
            this.ApplicationId = applicationId;
            this.CostBlockId = costBlockId;
            this.CostElementId = costElementId;
        }

        public CostElementIdentifier(ICostElementIdentifier costElementIdentifier)
        {
            this.ApplicationId = costElementIdentifier.ApplicationId;
            this.CostBlockId = costElementIdentifier.CostBlockId;
            this.CostElementId = costElementIdentifier.CostElementId;
        }
    }
}
