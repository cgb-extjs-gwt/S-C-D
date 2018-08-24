﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Dto.AvailabilityFee
{
    public class AdminAvailabilityFeeDto
    {
        public long? Id { get; set; }

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