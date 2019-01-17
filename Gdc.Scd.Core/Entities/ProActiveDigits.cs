using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table("ProActiveDigits", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class ProActiveDigit : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public long? DigitId { get; set; }
        public string Description { get; set; }
        public string ProActive { get; set; }
        public long? ProActiveId { get; set; }
        public string ReactiveMappingDigit { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
