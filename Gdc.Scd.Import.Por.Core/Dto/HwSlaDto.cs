using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class HwSlaDto
    {
        public Dictionary<string, List<long>> Countries { get; set; }
        public List<Wg> Wgs { get; set; }
        public List<Sog> Sogs { get; set; }
        public Dictionary<string, long> Proactive { get; set; }
    }
}
