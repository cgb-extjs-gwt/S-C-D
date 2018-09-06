using Gdc.Scd.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Entities.CapabilityMatrix
{
    [Table("MatrixRule")]
    public class CapabilityMatrixRule : IIdentifiable
    {
        public long Id { get; set; }

        public Country Country { get; set; }

        public Wg Wg { get; set; }

        public Availability Availability { get; set; }

        public Duration Duration { get; set; }

        public ReactionType ReactionType { get; set; }

        public ReactionTime ReactionTime { get; set; }

        public ServiceLocation ServiceLocation { get; set; }

        public bool FujitsuGlobalPortfolio { get; set; }

        public bool MasterPortfolio { get; set; }

        public bool CorePortfolio { get; set; }
    }
}
