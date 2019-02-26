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
    
    public partial class Hardware_FieldServiceCost
    {
        public long Id { get; set; }
        public Nullable<long> Country { get; set; }
        public Nullable<long> Pla { get; set; }
        public Nullable<long> CentralContractGroup { get; set; }
        public Nullable<long> Wg { get; set; }
        public Nullable<long> ServiceLocation { get; set; }
        public Nullable<long> ReactionTimeType { get; set; }
        public Nullable<double> RepairTime { get; set; }
        public Nullable<double> TravelTime { get; set; }
        public Nullable<double> LabourCost { get; set; }
        public Nullable<double> TravelCost { get; set; }
        public Nullable<double> PerformanceRate { get; set; }
        public Nullable<double> TimeAndMaterialShare { get; set; }
        public long CostBlockHistory { get; set; }
    
        public virtual ServiceLocation ServiceLocation1 { get; set; }
        public virtual CostBlockHistory CostBlockHistory1 { get; set; }
        public virtual CentralContractGroup1 CentralContractGroup1 { get; set; }
        public virtual Country1 Country1 { get; set; }
        public virtual Pla1 Pla1 { get; set; }
        public virtual Wg1 Wg1 { get; set; }
    }
}