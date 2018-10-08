using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities
{
    [Table("SoftwareProActive", Schema = MetaConstants.FspCodeTranslationSchema)]
    public class SoftwareProactive : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public SwDigit SwDigit { get; set; }
        public long SwDigitId { get; set; }

        public SwDigit ReactiveDigit { get; set; }
        public long? ReactiveDigitId { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public ProActiveSla ProActiveSla { get; set; }
        public long ProActiveSlaId { get; set; }

        public string ProActive { get; set; }
    }
}
