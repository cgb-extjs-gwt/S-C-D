using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table("StandardWarrantyManualCost", Schema = MetaConstants.HardwareSchema)]
    public class StandardWarrantyManualCost : IIdentifiable
    {
        public long Id { get; set; }

        public long CountryId { get; set; }
        public Country Country { get; set; }

        public long WgId { get; set; }
        public Wg Wg { get; set; }

        //ChangeUserId hack for correct save
        //TODO: remove ChangeUserId
        public long ChangeUserId { get; set; }
        public User ChangeUser { get; set; }

        public DateTime ChangeDate { get; set; }

        public double? StandardWarranty { get; set; }
    }
}
