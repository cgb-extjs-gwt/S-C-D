using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [MustUpdateCoordinate(MetaConstants.SwDigitInputLevel)]
    [Table(MetaConstants.SwDigitInputLevel, Schema = MetaConstants.InputLevelSchema)]
    public class SwDigit : NamedId, IModifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        [MustCompare(true, IsIgnoreCase = true)]
        public string Description { get; set; }

        [MustCompare]
        [MustUpdateCoordinate(MetaConstants.SogInputLevel)]
        public long SogId { get; set; }
        public Sog Sog { get; set; }

        [NotMapped]
        [MustUpdateCoordinate(MetaConstants.PlaInputLevelName)]
        public long? PlaId
        {
            get { return this.Sog?.PlaId; }
        }

        public DateTime CreatedDateTime { get; set; }

        public DateTime? DeactivatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public ICollection<SwDigitLicense> SwDigitLicenses { get; set; }
    }
}
