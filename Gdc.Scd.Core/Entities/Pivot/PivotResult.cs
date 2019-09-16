namespace Gdc.Scd.Core.Entities.Pivot
{
    public class PivotResult
    {
        public bool Success { get; set; }

        public ResultAxisItem[] LeftAxis { get; set; }

        public ResultAxisItem[] TopAxis { get; set; }

        public ResultItem[] Results { get; set; }
    }
}
