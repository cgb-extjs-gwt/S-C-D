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
    [Table("SwDigitLicense", Schema = MetaConstants.InputLevelSchema)]
    public class SwDigitLicense : IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? SwDigitId { get; set; }
        public long? SwLicenseId { get; set; }
        public string SwFspCode { get; set; }

        public SwDigit SwDigit { get; set; }
        public SwLicense SwLicense { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
