﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gdc.Scd.DataAccessLayer.External.Por
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FrieseEntities : DbContext
    {
        public FrieseEntities()
            : base("name=FrieseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<SCD2_ServiceOfferingGroups> SCD2_ServiceOfferingGroups { get; set; }
        public virtual DbSet<SCD2_SW_Overview> SCD2_SW_Overview { get; set; }
        public virtual DbSet<SCD2_v_SAR_new_codes> SCD2_v_SAR_new_codes { get; set; }
        public virtual DbSet<SCD2_WarrantyGroups> SCD2_WarrantyGroups { get; set; }
        public virtual DbSet<SCD2_LUT_TSP> SCD2_LUT_TSP { get; set; }
    }
}
