using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("Sog", Schema = MetaConstants.InputLevelSchema)]
    public class Sog : BaseWgSog, IDeactivatable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
