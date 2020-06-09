using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class AvailabilityWeight : DayHour, IIdentifiable
    {
        public long Id { get; set; }

        public double Weight { get; set; }
    }
}
