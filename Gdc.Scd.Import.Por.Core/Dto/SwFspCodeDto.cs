using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class SwFspCodeDto
    {
        public List<SCD2_v_SAR_new_codes> SoftwareCodes { get; set; }
        public List<Sog> Sogs { get; set; }
        public List<SwDigit> Digits { get; set; }
        public List<SwLicense> License { get; set; }
        public List<SCD2_SW_Overview> SoftwareInfo { get; set; }
        public SlaDictsDto Sla { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string[] SoftwareServiceTypes { get; set; }
        public List<ProActiveDigit> ProActiveDigits { get; set; }
    }
}
