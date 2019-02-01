namespace Gdc.Scd.Core.Entities
{
    public class CostEditorContext : CostElementContext
    {
        public long[] CostElementFilterIds { get; set; }

        public long[] InputLevelFilterIds { get; set; }
    }
}
