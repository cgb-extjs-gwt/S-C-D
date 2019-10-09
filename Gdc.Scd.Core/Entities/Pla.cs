using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.PlaInputLevelName, Schema = MetaConstants.InputLevelSchema)]
    public class Pla : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string CodingPattern { get; set; }

        [MustUpdateCoordinate(MetaConstants.CompanyInputLevelName)]
        public long CompanyId { get; set; }
        public Company Company { get; set; }

        public List<Wg> WarrantyGroups { get; set; }
    }
}
