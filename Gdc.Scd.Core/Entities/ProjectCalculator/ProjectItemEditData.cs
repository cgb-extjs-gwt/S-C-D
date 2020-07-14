namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class ProjectItemEditData
    {
        public NamedId[] Wgs { get; set; }

        public NamedId[] Countries { get; set; }

        public NamedId[] ReactionTimePeriods { get; set; }

        public NamedId[] ReactionTypes { get; set; }

        public NamedId[] ServiceLocations { get; set; }

        public NamedId[] DurationPeriods { get; set; }

        public NamedId[] ReinsuranceCurrencies { get; set; }
    }
}
