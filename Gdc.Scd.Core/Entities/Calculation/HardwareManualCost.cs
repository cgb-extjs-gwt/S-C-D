using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.Calculation
{
    [Table(MetaConstants.ManualCostTable, Schema = MetaConstants.HardwareSchema)]
    public class HardwareManualCost : IIdentifiable
    {
        [ForeignKey("LocalPortfolio")]
        [Column("PortfolioId")]
        public long Id { get; set; }

        public LocalPortfolio LocalPortfolio { get; set; }

        //ChangeUserId hack for correct save
        //TODO: remove ChangeUserId
        public long? ChangeUserId { get; set; }
        public User ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public double? ServiceTC { get; set; }

        public double? ServiceTP { get; set; }

        public double? ListPrice { get; set; }

        public double? DealerDiscount { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public double? DealerPrice { get; private set; }

        public double? ServiceTP1_Released { get; set; }
        public double? ServiceTP2_Released { get; set; }
        public double? ServiceTP3_Released { get; set; }
        public double? ServiceTP4_Released { get; set; }
        public double? ServiceTP5_Released { get; set; }

        public double? ServiceTP_Released { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public DateTime? SapUploadDate { get; set; }

        public DateTime? NextSapUploadDate { get; set; }

        //ReleaseUserId hack for correct save
        //TODO: remove ReleaseUserId
        public long? ReleaseUserId { get; set; }
        public User ReleaseUser { get; set; }

    }
}
