//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gdc.Scd.CopyDataTool.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class PrincipalPortfolio
    {
        public long Id { get; set; }
        public long AvailabilityId { get; set; }
        public long DurationId { get; set; }
        public long ProActiveSlaId { get; set; }
        public long ReactionTimeId { get; set; }
        public long ReactionTypeId { get; set; }
        public long ServiceLocationId { get; set; }
        public long WgId { get; set; }
        public bool IsCorePortfolio { get; set; }
        public bool IsGlobalPortfolio { get; set; }
        public bool IsMasterPortfolio { get; set; }
    
        public virtual ServiceLocation ServiceLocation { get; set; }
        public virtual Wg Wg { get; set; }
    }
}