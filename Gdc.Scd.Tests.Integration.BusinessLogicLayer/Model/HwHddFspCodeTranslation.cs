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
    
    public partial class HwHddFspCodeTranslation
    {
        public long Id { get; set; }
        public Nullable<long> CountryId { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public string EKKey { get; set; }
        public string EKSAPKey { get; set; }
        public string Name { get; set; }
        public string SCD_ServiceType { get; set; }
        public string SecondSLA { get; set; }
        public string ServiceDescription { get; set; }
        public string Status { get; set; }
        public long WgId { get; set; }
    
        public virtual Country1 Country1 { get; set; }
        public virtual Wg1 Wg1 { get; set; }
    }
}
