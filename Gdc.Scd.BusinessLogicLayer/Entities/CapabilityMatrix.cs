using Gdc.Scd.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public abstract class CapabilityMatrix : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public Country Country { get; set; }

        public Wg Wg { get; set; }

        public Availability Availability { get; set; }

        public Duration Duration { get; set; }

        public ReactionType ReactionType { get; set; }

        public ReactionTime ReactionTime { get; set; }

        public ServiceLocation ServiceLocation { get; set; }

        public bool FujitsuGlobalPortfolio { get; set; }

        public bool MasterPortfolio { get; set; }

        public string Hash { get; set; }
    }
}