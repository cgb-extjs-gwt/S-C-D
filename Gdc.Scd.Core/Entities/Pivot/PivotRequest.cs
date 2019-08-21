namespace Gdc.Scd.Core.Entities.Pivot
{
    public class PivotRequest
    {
        public string KeysSeparator { get; set; }

        public string GrandTotalKey { get; set; }

        public RequestAxisItem[] LeftAxis { get; set; }

        public RequestAxisItem[] TopAxis { get; set; }

        public RequestAxisItem[] Aggregate { get; set; }
    }
}
