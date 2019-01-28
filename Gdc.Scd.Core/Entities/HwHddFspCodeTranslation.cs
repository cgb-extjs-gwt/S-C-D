using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.HwHddFspCodeTranslationTable, Schema = MetaConstants.FspCodeTranslationSchema)]
    public class HwHddFspCodeTranslation : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string SCD_ServiceType { get; set; }

        public string ServiceDescription { get; set; }

        public string EKSAPKey { get; set; }

        public string EKKey { get; set; }

        public string Status { get; set; }

        public string SecondSLA { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public Country Country { get; set; }
        public long? CountryId { get; set; }

        public Wg Wg { get; set; }
        public long WgId { get; set; }
    }
}
