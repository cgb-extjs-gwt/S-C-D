using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class ReactionTimeProjCalc
    { 
        public string Name { get; set; }

        public int? Minutes { get; set; }

        public PeriodType? PeriodType { get; set; }

        [NotMapped]
        public int? Value { get; set; }
    }
}
