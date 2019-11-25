using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class HwFspCodeDto
    {
        public List<SCD2_v_SAR_new_codes> HardwareCodes { get; set; }
        public List<SCD2_v_SAR_new_codes> ProactiveCodes { get; set; }
        public List<SCD2_v_SAR_new_codes> StandardWarranties { get; set; }
        public List<SCD2_v_SAR_new_codes> HddRetentionCodes { get; set; }
        public List<SCD2_LUT_TSP> LutCodes { get; set; }
        public HwSlaDto HwSla { get; set; }
        public SlaDictsDto Sla { get; set; }
        public DateTime CreationDate { get; set; }
        public string[] ProactiveServiceTypes { get; set; }
        public string[] StandardWarrantiesServiceTypes { get; set; }
        public string[] OtherHardwareServiceTypes { get; set; }
    }
}
