namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class CostEditorContext
    {
        public string ApplicationId { get; set; }

        public string RegionInputId { get; set; }

        public string CostBlockId { get; set; }

        public string CostElementId { get; set; }

        public string InputLevelId { get; set; }

        public long[] CostElementFilterIds { get; set; }

        public long[] InputLevelFilterIds { get; set; }
    }
}
