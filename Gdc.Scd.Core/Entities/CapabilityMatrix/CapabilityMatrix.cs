using Gdc.Scd.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.CapabilityMatrix
{
    [Table("Matrix")]
    public class CapabilityMatrix : IIdentifiable
    {
        public long Id { get; set; }

        #region Dependencies, denormalized

        public long? CountryId { get; set; }
        public string Country { get; set; }
        public string CountryGroup { get; set; }
        public Country CountryRef { get; set; }

        public long WgId { get; set; }
        [Required]
        public string Wg { get; set; }
        [Required]
        public string WgDescription { get; set; }
        [Required]
        public Wg WgRef { get; set; }

        public long AvailabilityId { get; set; }
        [Required]
        public string Availability { get; set; }
        [Required]
        public Availability AvailabilityRef { get; set; }

        public long DurationId { get; set; }
        [Required]
        public string Duration { get; set; }
        [Required]
        public string DurationValue { get; set; }
        [Required]
        public Duration DurationRef { get; set; }

        public long ReactionTypeId { get; set; }
        [Required]
        public string ReactionType { get; set; }
        [Required]
        public ReactionType ReactionTypeRef { get; set; }

        public long ReactionTimeId { get; set; }
        [Required]
        public string ReactionTime { get; set; }
        [Required]
        public ReactionTime ReactionTimeRef { get; set; }

        public long ServiceLocationId { get; set; }
        [Required]
        public string ServiceLocation { get; set; }
        [Required]
        public ServiceLocation ServiceLocationRef { get; set; }

        #endregion

        public bool FujitsuGlobalPortfolio { get; set; }

        public bool MasterPortfolio { get; set; }

        public bool CorePortfolio { get; set; }

        public bool Denied { get; set; }
    }
}