using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    [Table("SwSpMaintenance", Schema = MetaConstants.SoftwareSolutionSchema)]
    public class SwSpMaintenance : IIdentifiable
    {
        public long Id { get; set; }

        public long? Pla { get; set; }
        public long? Sfab { get; set; }
        public long? Sog { get; set; }
        public long? SwDigit { get; set; }
        [Column("2ndLevelSupportCosts")]
        public double? C2ndLevelSupportCosts { get; set; }
        [Column("2ndLevelSupportCosts_Approved")]
        public double? C2ndLevelSupportCosts_Approved { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }
}
