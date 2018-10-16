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
    [Table("InstallBase", Schema = MetaConstants.AtomSchema)]
    public class InstallBase : IIdentifiable, IDeactivatable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("Wg")]
        public long? WgId { get; set; }
        public Wg Wg { get; set; }

        [Column("Pla")]
        public long? PlaId { get; set; }
        public Pla Pla { get; set; }

        [Column("Country")]
        public long? CountryId { get; set; }
        public Country Country { get; set; }

        public double? InstalledBaseCountry { get; set; }
        public double? InstalledBaseCountry_Approved { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }
}
