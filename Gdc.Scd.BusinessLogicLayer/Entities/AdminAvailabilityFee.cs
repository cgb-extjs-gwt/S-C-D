using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    [Table("AvailabilityFee", Schema = MetaConstants.AdminSchema)]
    public class AdminAvailabilityFee : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public Country Country { get; set; }

        public ReactionTime ReactionTime {get;set;}

        public ReactionType ReactionType { get; set; }

        public ServiceLocation ServiceLocation { get; set; }
    }
}
