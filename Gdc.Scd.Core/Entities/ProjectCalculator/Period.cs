namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public abstract class Period
    {
        public int Minutes { get; set; }

        public PeriodType PeriodType { get; set; }
    }
}
