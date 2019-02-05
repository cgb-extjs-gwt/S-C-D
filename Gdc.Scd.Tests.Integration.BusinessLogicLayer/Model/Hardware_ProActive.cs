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
    
    public partial class Hardware_ProActive
    {
        public long Id { get; set; }
        public Nullable<long> Country { get; set; }
        public Nullable<long> Pla { get; set; }
        public Nullable<long> CentralContractGroup { get; set; }
        public Nullable<long> Wg { get; set; }
        public Nullable<double> LocalRemoteAccessSetupPreparationEffort { get; set; }
        public Nullable<double> LocalRegularUpdateReadyEffort { get; set; }
        public Nullable<double> LocalPreparationShcEffort { get; set; }
        public Nullable<double> CentralExecutionShcReportCost { get; set; }
        public Nullable<double> LocalRemoteShcCustomerBriefingEffort { get; set; }
        public Nullable<double> LocalOnSiteShcCustomerBriefingEffort { get; set; }
        public Nullable<double> TravellingTime { get; set; }
        public Nullable<double> OnSiteHourlyRate { get; set; }
        public long CostBlockHistory { get; set; }
    
        public virtual CostBlockHistory CostBlockHistory1 { get; set; }
        public virtual CentralContractGroup1 CentralContractGroup1 { get; set; }
        public virtual Country1 Country1 { get; set; }
        public virtual Pla1 Pla1 { get; set; }
        public virtual Wg1 Wg1 { get; set; }
    }
}
