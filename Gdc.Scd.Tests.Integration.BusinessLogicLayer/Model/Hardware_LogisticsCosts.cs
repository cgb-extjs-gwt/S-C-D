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
    
    public partial class Hardware_LogisticsCosts
    {
        public long Id { get; set; }
        public Nullable<long> Country { get; set; }
        public Nullable<long> Pla { get; set; }
        public Nullable<long> CentralContractGroup { get; set; }
        public Nullable<long> Wg { get; set; }
        public Nullable<long> ReactionTimeType { get; set; }
        public Nullable<double> StandardHandling { get; set; }
        public Nullable<double> HighAvailabilityHandling { get; set; }
        public Nullable<double> StandardDelivery { get; set; }
        public Nullable<double> ExpressDelivery { get; set; }
        public Nullable<double> TaxiCourierDelivery { get; set; }
        public Nullable<double> ReturnDeliveryFactory { get; set; }
        public long CostBlockHistory { get; set; }
    
        public virtual CostBlockHistory CostBlockHistory1 { get; set; }
        public virtual CentralContractGroup1 CentralContractGroup1 { get; set; }
        public virtual Country1 Country1 { get; set; }
        public virtual Pla1 Pla1 { get; set; }
        public virtual Wg1 Wg1 { get; set; }
    }
}