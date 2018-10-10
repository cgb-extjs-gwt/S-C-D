using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities
{
    [Table("TaxAndDuties", Schema = MetaConstants.AtomSchema)]
    public class TaxAndDutiesEntity : IDeactivatable, IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        
        public Country Country { get; set; }

        [Column("Country")]
        public long CountryId {get;set;}

        public double? TaxAndDuties { get; set; }
        public double? TaxAndDuties_Approved { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
