namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class ProjectCalculatorData
    {
        public Wg[] Wgs { get; set; }

        public Country[] Countries { get; set; }

        public NamedId[] ReactionTimePeriods { get; set; }

        public ReactionType[] ReactionTypes { get; set; }

        public ServiceLocation[] ServiceLocations { get; set; }

        public NamedId[] DurationPeriods { get; set; }
    }
}
