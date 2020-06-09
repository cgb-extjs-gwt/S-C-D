using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    [Table("Afr", Schema = MetaConstants.ProjectCalculatorSchema)]
    public class AfrProjCalc : IIdentifiable
    {
        public long Id { get; set; }

        public double? AFR { get; set; }

        public int Months { get; set; }

        public bool IsProlongation { get; set; }
    }
}
