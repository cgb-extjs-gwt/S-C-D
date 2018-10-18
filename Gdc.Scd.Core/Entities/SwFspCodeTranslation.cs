using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

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

        public Sog Sog { get; set; }
        public long SogId { get; set; }

        public ProActiveSla ProActiveSla { get; set; }
        public long? ProactiveSlaId { get; set; }

    }
}
