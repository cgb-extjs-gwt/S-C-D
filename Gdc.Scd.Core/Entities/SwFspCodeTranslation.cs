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
    [Table("SwFspCodeTranslation", Schema = MetaConstants.FspCodeTranslationSchema)]
    public class SwFspCodeTranslation : FspCodeTranslation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public SwDigit SwDigit { get; set; }
        public long? SwDigitId { get; set; }
    }
}
