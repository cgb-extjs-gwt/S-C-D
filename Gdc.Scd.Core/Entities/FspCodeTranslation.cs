using System;

namespace Gdc.Scd.Core.Entities
{
    public class FspCodeTranslation : NamedId
    {
        public string ServiceDescription { get; set; }

        public string EKSAPKey { get; set; }

        public string EKKey { get; set; }

        public string ServiceType { get; set; }

        public string SCD_ServiceType { get; set; }

        public string Status { get; set; }

        public string SecondSLA { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public ServiceLocation ServiceLocation { get; set; }
        public long? ServiceLocationId { get; set; }

        public ReactionTime ReactionTime { get; set; }
        public long? ReactionTimeId { get; set; }

        public ReactionType ReactionType { get; set; }
        public long? ReactionTypeId { get; set; }

        public Availability Availability { get; set; }
        public long? AvailabilityId { get; set; }

        public Duration Duration { get; set; }
        public long? DurationId { get; set; }
    }
}
