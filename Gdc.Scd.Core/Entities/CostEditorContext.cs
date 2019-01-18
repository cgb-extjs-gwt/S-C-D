namespace Gdc.Scd.Core.Entities
{
    public class CostEditorContext : HistoryContext
    {
        public long[] CostElementFilterIds { get; set; }

        public long[] InputLevelFilterIds { get; set; }
    }
}
