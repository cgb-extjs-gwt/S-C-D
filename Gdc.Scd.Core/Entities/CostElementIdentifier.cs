using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class CostElementIdentifier : ICostElementIdentifier
    {
        public string CostElementId { get; set; }

        public string ApplicationId { get; set; }

        public string CostBlockId { get; set; }
    }
}
