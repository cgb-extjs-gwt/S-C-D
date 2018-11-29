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
    
    public partial class Contract_Result
    {
        public long Id { get; set; }
        public string Country { get; set; }
        public string Wg { get; set; }
        public string WgDescription { get; set; }
        public Nullable<int> SLA { get; set; }
        public string ServiceLocation { get; set; }
        public string ReactionTime { get; set; }
        public string ReactionType { get; set; }
        public string Availability { get; set; }
        public Nullable<double> ServiceTP1 { get; set; }
        public Nullable<double> ServiceTP2 { get; set; }
        public Nullable<double> ServiceTP3 { get; set; }
        public Nullable<double> ServiceTP4 { get; set; }
        public Nullable<double> ServiceTP5 { get; set; }
        public Nullable<double> ServiceTPMonthly1 { get; set; }
        public Nullable<double> ServiceTPMonthly2 { get; set; }
        public Nullable<double> ServiceTPMonthly3 { get; set; }
        public Nullable<double> ServiceTPMonthly4 { get; set; }
        public Nullable<double> ServiceTPMonthly5 { get; set; }
        public Nullable<int> WarrantyLevel { get; set; }
        public Nullable<int> PortfolioType { get; set; }
        public string Sog { get; set; }
    }
}