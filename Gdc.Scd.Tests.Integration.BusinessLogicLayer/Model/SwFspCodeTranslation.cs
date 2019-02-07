//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class SwFspCodeTranslation
    {
        public long Id { get; set; }
        public Nullable<long> AvailabilityId { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public Nullable<long> DurationId { get; set; }
        public string EKKey { get; set; }
        public string EKSAPKey { get; set; }
        public string Name { get; set; }
        public Nullable<long> ProactiveSlaId { get; set; }
        public Nullable<long> ReactionTimeId { get; set; }
        public Nullable<long> ReactionTypeId { get; set; }
        public string SCD_ServiceType { get; set; }
        public string SecondSLA { get; set; }
        public string ServiceDescription { get; set; }
        public Nullable<long> ServiceLocationId { get; set; }
        public string ServiceType { get; set; }
        public long SogId { get; set; }
        public string Status { get; set; }
        public Nullable<long> SwDigitId { get; set; }
    
        public virtual Availability Availability { get; set; }
        public virtual Duration Duration { get; set; }
        public virtual ProActiveSla ProActiveSla { get; set; }
        public virtual ReactionTime ReactionTime { get; set; }
        public virtual ReactionType ReactionType { get; set; }
        public virtual ServiceLocation ServiceLocation { get; set; }
        public virtual Sog1 Sog1 { get; set; }
        public virtual SwDigit1 SwDigit1 { get; set; }
    }
}
