namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class AvailabilityProjCalc
    {
        public string Name { get; set; }

        public DayHour Start { get; set; }

        public DayHour End { get; set; }

        public int Value { get; set; }

        public override string ToString()
        {
            return $"{this.Start.Day} - {this.End.Day} ({this.Start.Hour}:00-{this.End.Hour + 1}:00)";
        }
    }
}
