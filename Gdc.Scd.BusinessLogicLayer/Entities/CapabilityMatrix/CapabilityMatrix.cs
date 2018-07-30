using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix
{
    public abstract class CapabilityMatrix : IIdentifiable
    {
        public virtual long Id { get; set; }

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

        public string Hash { get; set; }
    }
}