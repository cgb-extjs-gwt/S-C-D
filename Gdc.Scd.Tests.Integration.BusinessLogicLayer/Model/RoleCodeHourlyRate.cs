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
    
    public partial class RoleCodeHourlyRate
    {
        public long Id { get; set; }
        public long RoleCode { get; set; }
        public Nullable<double> OnsiteHourlyRates { get; set; }
        public Nullable<double> OnsiteHourlyRates_Approved { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public Nullable<System.DateTime> DeactivatedDateTime { get; set; }
    
        public virtual RoleCode RoleCode1 { get; set; }
    }
}