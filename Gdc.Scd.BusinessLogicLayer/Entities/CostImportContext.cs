using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class CostImportContext : CostElementIdentifier
    {
        public string InputLevelId { get; set; }

        public long? DependencyItemId { get; set; }

        public long? RegionId { get; set; }
    }
}
