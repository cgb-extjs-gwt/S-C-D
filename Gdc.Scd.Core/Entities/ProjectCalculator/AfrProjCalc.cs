using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    [Table("Afr", Schema = MetaConstants.ProjectCalculatorSchema)]
    public class AfrProjCalc 
    {
        public long Id { get; set; }

        public double? AFR { get; set; }

        public long? WgId { get; set; }

        public Wg Wg { get; set; }

        public int Months { get; set; }
    }
}
