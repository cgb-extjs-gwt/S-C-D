using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class SwProActiveDto
    {
        public Dictionary<string, long> Proactive { get; set; }
        public List<SCD2_SWR_Level> ProActiveInfo { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<SwDigit> SwDigits { get; set; }
    }
}
