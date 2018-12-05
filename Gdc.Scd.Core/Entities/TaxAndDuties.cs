using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("TaxAndDuties", Schema = MetaConstants.HardwareSchema)]
    public class TaxAndDutiesEntity : IIdentifiable, IDeactivatable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        public Country Country { get; set; }

        [Column(MetaConstants.CountryInputLevelName)]
        public long CountryId {get;set;}

        public double? TaxAndDuties { get; set; }
        public double? TaxAndDuties_Approved { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
