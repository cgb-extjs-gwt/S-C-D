using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class HwHddFspCodeDto
    {
        public List<SCD2_v_SAR_new_codes> HardwareCodes { get; set; }
        public DateTime CreationDate { get; set; }
        public HwSlaDto HwSla { get; set; }
    }
}
