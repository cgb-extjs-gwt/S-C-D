using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class CostEditorContext : ICostElementIdentifier
    {
        public string ApplicationId { get; set; }

        public long? RegionInputId { get; set; }

        public string CostBlockId { get; set; }

        public string CostElementId { get; set; }

        public string InputLevelId { get; set; }

        public long[] CostElementFilterIds { get; set; }

        public long[] InputLevelFilterIds { get; set; }
    }
}
