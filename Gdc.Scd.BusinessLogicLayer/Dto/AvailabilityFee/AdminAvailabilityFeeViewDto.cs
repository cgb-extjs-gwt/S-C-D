using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee
{
    public class AdminAvailabilityFeeViewDto
    {
        public bool IsApplicable { get; set; }

        public long InnerId { get; set; }

        public string CountryName { get; set; }
        public long CountryId { get; set; }

        public string ReactionTimeName { get; set; }
        public long ReactionTimeId { get; set; }

        public string ReactionTypeName { get; set; }
        public long ReactionTypeId { get; set; }

        public string ServiceLocatorName { get; set; }
        public long ServiceLocatorId { get; set; }
    }
}
