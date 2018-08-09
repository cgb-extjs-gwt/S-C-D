using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix
{
    public abstract class CapabilityMatrixView : IIdentifiable
    {
        public long Id { get; set; }

        public long WgId { get; set; }
        public string Wg { get; set; }

        public long AvailabilityId { get; set; }
        public string Availability { get; set; }

        public long DurationId { get; set; }
        public string Duration { get; set; }

        public long ReactionTypeId { get; set; }
        public string ReactionType { get; set; }

        public long ReactionTimeId { get; set; }
        public string ReactionTime { get; set; }

        public long ServiceLocationId { get; set; }
        public string ServiceLocation { get; set; }
    }

}
