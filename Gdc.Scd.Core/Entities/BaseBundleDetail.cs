namespace Gdc.Scd.Core.Entities
{
    public abstract class BaseBundleDetail
    {
        public long HistoryValueId { get; set; }

        public NamedId Wg { get; set; }

        public object NewValue { get; set; }

        public double? OldValue { get; set; }

        public double? CountryGroupAvgValue { get; set; }

        public bool IsRegionError { get; set; }

        public bool IsPeriodError { get; set; }
    }
}
