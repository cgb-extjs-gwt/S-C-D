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
    
    public partial class SwSpMaintenance
    {
        public long Id { get; set; }
        public long Pla { get; set; }
        public long Sfab { get; set; }
        public long Sog { get; set; }
        public long YearAvailability { get; set; }
        public long Availability { get; set; }
        public Nullable<double> C2ndLevelSupportCosts { get; set; }
        public Nullable<double> InstalledBaseSog { get; set; }
        public Nullable<double> ReinsuranceFlatfee { get; set; }
        public Nullable<long> CurrencyReinsurance { get; set; }
        public Nullable<double> RecommendedSwSpMaintenanceListPrice { get; set; }
        public Nullable<double> MarkupForProductMarginSwLicenseListPrice { get; set; }
        public Nullable<double> ShareSwSpMaintenanceListPrice { get; set; }
        public Nullable<double> DiscountDealerPrice { get; set; }
        public Nullable<double> C2ndLevelSupportCosts_Approved { get; set; }
        public Nullable<double> InstalledBaseSog_Approved { get; set; }
        public Nullable<double> ReinsuranceFlatfee_Approved { get; set; }
        public Nullable<long> CurrencyReinsurance_Approved { get; set; }
        public Nullable<double> RecommendedSwSpMaintenanceListPrice_Approved { get; set; }
        public Nullable<double> MarkupForProductMarginSwLicenseListPrice_Approved { get; set; }
        public Nullable<double> ShareSwSpMaintenanceListPrice_Approved { get; set; }
        public Nullable<double> DiscountDealerPrice_Approved { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public Nullable<System.DateTime> DeactivatedDateTime { get; set; }
    
        public virtual Availability Availability1 { get; set; }
        public virtual Year_Availability Year_Availability { get; set; }
        public virtual Pla Pla1 { get; set; }
        public virtual Sfab Sfab1 { get; set; }
        public virtual Sog Sog1 { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual Currency Currency1 { get; set; }
    }
}