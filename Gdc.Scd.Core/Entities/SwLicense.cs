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

        public long SwDigitId { get; set; }
        public SwDigit SwDigit { get; set; }

        public string SoftwareLicense { get; set; }
        public string SoftwareLicenseName { get; set; }
        public string SoftwareLicenseDescription { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
