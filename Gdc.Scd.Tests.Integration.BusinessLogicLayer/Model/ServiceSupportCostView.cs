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
    
    public partial class ServiceSupportCostView
    {
        public long Country { get; set; }
        public long Wg { get; set; }
        public int IsMultiVendor { get; set; }
        public long ClusterRegion { get; set; }
        public long ClusterPla { get; set; }
        public Nullable<double> C1stLevelSupportCosts { get; set; }
        public Nullable<double> C1stLevelSupportCosts_Approved { get; set; }
        public Nullable<double> C2ndLevelSupportCosts { get; set; }
        public Nullable<double> C2ndLevelSupportCosts_Approved { get; set; }
    }
}