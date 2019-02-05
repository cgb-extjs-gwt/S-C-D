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
    
    public partial class CentralContractGroup1
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CentralContractGroup1()
        {
            this.FieldServiceCosts = new HashSet<FieldServiceCost>();
            this.LogisticsCosts = new HashSet<LogisticsCost>();
            this.MarkupOtherCosts = new HashSet<MarkupOtherCost>();
            this.MarkupStandardWaranties = new HashSet<MarkupStandardWaranty>();
            this.ProActives = new HashSet<ProActive>();
            this.ProlongationMarkups = new HashSet<ProlongationMarkup>();
            this.Hardware_FieldServiceCost = new HashSet<Hardware_FieldServiceCost>();
            this.Hardware_LogisticsCosts = new HashSet<Hardware_LogisticsCosts>();
            this.Hardware_MarkupOtherCosts = new HashSet<Hardware_MarkupOtherCosts>();
            this.Hardware_MarkupStandardWaranty = new HashSet<Hardware_MarkupStandardWaranty>();
            this.Hardware_ProActive = new HashSet<Hardware_ProActive>();
            this.Hardware_ProlongationMarkup = new HashSet<Hardware_ProlongationMarkup>();
            this.CentralContractGroups = new HashSet<CentralContractGroup>();
            this.Wg1 = new HashSet<Wg1>();
        }
    
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldServiceCost> FieldServiceCosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LogisticsCost> LogisticsCosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarkupOtherCost> MarkupOtherCosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarkupStandardWaranty> MarkupStandardWaranties { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProActive> ProActives { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProlongationMarkup> ProlongationMarkups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hardware_FieldServiceCost> Hardware_FieldServiceCost { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hardware_LogisticsCosts> Hardware_LogisticsCosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hardware_MarkupOtherCosts> Hardware_MarkupOtherCosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hardware_MarkupStandardWaranty> Hardware_MarkupStandardWaranty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hardware_ProActive> Hardware_ProActive { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hardware_ProlongationMarkup> Hardware_ProlongationMarkup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CentralContractGroup> CentralContractGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Wg1> Wg1 { get; set; }
    }
}
