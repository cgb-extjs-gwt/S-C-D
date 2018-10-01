using Gdc.Scd.Core.Attributes;
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
    [Table("SwLicense", Schema = MetaConstants.InputLevelSchema)]
    public class SwLicense : NamedId, IDeactivatable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        [MustCompare(true, IsIgnoreCase = true)]
        public string Description { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public ICollection<SwDigitLicense> SwDigitLicenses { get; set; }
    }
}
