using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class DurationProjCalc
    {
        public string Name { get; set; }

        public int Months { get; set; }

        public PeriodType PeriodType { get; set; }

        [NotMapped]
        public int? Value { get; set; }
    }
}
