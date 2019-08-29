using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.SfabInputLevel, Schema = MetaConstants.InputLevelSchema)]
    public class SFab : NamedId, IModifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        [MustCompare]
        public long PlaId { get; set; }
        public Pla Pla { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
