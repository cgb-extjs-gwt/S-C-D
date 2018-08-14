using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gdc.Scd.Core.Entities
{
    [Table("AvailabilityFee", Schema = MetaConstants.AdminSchema)]
    public class AdminAvailabilityFee : IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id {get;set;}

        public Country Country { get; set; }
        public long CountryId { get; set; }

        public ReactionTime ReactionTime {get;set;}
        public long ReactionTimeId { get; set; }

        public ReactionType ReactionType { get; set; }
        public long ReactionTypeId { get; set; }

        public ServiceLocation ServiceLocation { get; set; }
        public long ServiceLocationId { get; set; }
    }
}
