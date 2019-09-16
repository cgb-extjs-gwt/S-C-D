//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gdc.Scd.CopyDataTool.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProActiveSla
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProActiveSla()
        {
            this.LocalPortfolio = new HashSet<LocalPortfolio>();
        }
    
        public long Id { get; set; }
        public int CentralExecutionShcReportRepetition { get; set; }
        public string ExternalName { get; set; }
        public int LocalOnsiteShcCustomerBriefingRepetition { get; set; }
        public int LocalPreparationShcRepetition { get; set; }
        public int LocalRegularUpdateReadyRepetition { get; set; }
        public int LocalRemoteShcCustomerBriefingRepetition { get; set; }
        public string Name { get; set; }
        public int TravellingTimeRepetition { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LocalPortfolio> LocalPortfolio { get; set; }
    }
}
