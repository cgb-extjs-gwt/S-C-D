using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee
{
    public class AdminAvailabilityFeeDto
    {
        public long Id { get; set; }
        public string Country { get; set; }
        public string ReactionTime { get; set; }
        public string ReactionType { get; set; }
        public string ServiceLocator { get; set; }
        public bool IsApplicable { get; set; }
    }
}
