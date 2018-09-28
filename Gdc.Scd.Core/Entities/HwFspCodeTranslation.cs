using Gdc.Scd.Core.Meta.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("FspCodeTranslation", Schema = MetaConstants.FspCodeTranslationSchema)]
    public class HwFspCodeTranslation : FspCodeTranslation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public Country Country { get; set; }
        public long? CountryId { get; set; }

        public CapabilityMatrix.CapabilityMatrix Matrix { get; set; }
        public long? MatrixId { get; set; }

        public ProActiveSla ProActiveSla { get; set; }
        public long? ProactiveSlaId { get; set; }
    }
}
