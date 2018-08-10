using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee
{
    public class AdminAvailabilityFeeFilterDto
    {
        public List<AdminAvailabilityFeeViewDto> Combinations { get; set; }
        public int TotalCount { get; set; }

        public AdminAvailabilityFeeFilterDto()
        {
            Combinations = new List<AdminAvailabilityFeeViewDto>();
        }
    }
}
