namespace Gdc.Scd.Core.Entities.Approval
{
    public abstract class BaseBundleDetail
    {
        public long HistoryValueId { get; set; }

        public NamedId LastInputLevel { get; set; }

        public object NewValue { get; set; }

        public object OldValue { get; set; }

        public double? CountryGroupAvgValue { get; set; }

        public bool IsRegionError { get; set; }

        public bool IsPeriodError { get; set; }
    }
}
