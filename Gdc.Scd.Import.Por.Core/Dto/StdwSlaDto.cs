using Gdc.Scd.Core.Entities;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class StdwSlaDto
    {
        public Dictionary<string, List<long>> Countries { get; set; }
        public List<Wg> Wgs { get; set; }
    }
}
