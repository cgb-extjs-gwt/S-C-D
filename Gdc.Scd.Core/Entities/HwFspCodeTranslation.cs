using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.CapabilityMatrix;

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
