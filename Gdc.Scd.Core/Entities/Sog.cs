using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.SogInputLevel, Schema = MetaConstants.InputLevelSchema)]
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
