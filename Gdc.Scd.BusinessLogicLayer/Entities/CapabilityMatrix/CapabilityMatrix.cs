using Gdc.Scd.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix
{
    [Table("Matrix")]
    public class CapabilityMatrix : IIdentifiable
    {
        public long Id { get; set; }

        public Country Country { get; set; }

        [Required]
        public Wg Wg { get; set; }

        [Required]
        public Availability Availability { get; set; }

        [Required]
        public Duration Duration { get; set; }

        [Required]
        public ReactionType ReactionType { get; set; }

        [Required]
        public ReactionTime ReactionTime { get; set; }

        [Required]
        public ServiceLocation ServiceLocation { get; set; }

        public bool FujitsuGlobalPortfolio { get; set; }

        public bool MasterPortfolio { get; set; }

        public bool CorePortfolio { get; set; }

        public bool Denied { get; set; }
    }
}