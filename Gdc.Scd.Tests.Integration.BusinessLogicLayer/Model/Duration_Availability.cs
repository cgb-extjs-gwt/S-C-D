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
    
    public partial class Duration_Availability
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Duration_Availability()
        {
            this.SwSpMaintenances = new HashSet<SwSpMaintenance>();
        }
    
        public long Id { get; set; }
        public Nullable<long> AvailabilityId { get; set; }
        public bool IsDisabled { get; set; }
        public Nullable<long> YearId { get; set; }
    
        public virtual Availability Availability { get; set; }
        public virtual Duration Duration { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SwSpMaintenance> SwSpMaintenances { get; set; }
    }
}