namespace Gdc.Scd.Core.Entities
{
    public class HistoryContext
    {
        public string ApplicationId { get; set; }

        public long? RegionInputId { get; set; }

        public string CostBlockId { get; set; }

        public string CostElementId { get; set; }

        public string InputLevelId { get; set; }
    }
}
