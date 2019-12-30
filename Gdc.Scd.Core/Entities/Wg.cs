using System;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Entities
{
    [MustUpdateCoordinate(MetaConstants.WgInputLevelName)]
    [Table(MetaConstants.WgInputLevelName, Schema = MetaConstants.InputLevelSchema)]
    public class Wg : BaseWgSog, IModifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        [MustCompare]
        public long? SogId { get; set; }
        public Sog Sog { get; set; }

        public long? RoleCodeId { get; set; }

        public RoleCode RoleCode { get; set; }

        [MustCompare(true)]
        [MustUpdateCoordinate(MetaConstants.CompanyInputLevelName)]
        public long? CompanyId { get; set; }
        public Company Company { get; set; }

        [MustCompare]
        public WgType WgType { get; set; }

        public bool ExistsInLogisticsDb { get; set; }
        public bool IsDeactivatedInLogistic { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? DeactivatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string ResponsiblePerson { get; set; }

        public CentralContractGroup CentralContactGroup { get; set; }

        public long? CentralContractGroupId { get; set; }

        public bool PsmRelease { get; set; }

        public bool IsNotified { get; set; }
    }
}