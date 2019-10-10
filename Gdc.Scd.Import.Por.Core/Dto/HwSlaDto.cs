using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

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
