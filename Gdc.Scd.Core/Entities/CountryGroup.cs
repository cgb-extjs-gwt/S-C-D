using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table("CountryGroup", Schema = MetaConstants.InputLevelSchema)]
    public class CountryGroup : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string CountryDigit { get; set; }

        public string LUTCode { get; set; }

        public long RegionId { get; set; }

        public Region Region { get; set; }

        public List<Country> Countries { get; set; }
    }
}
