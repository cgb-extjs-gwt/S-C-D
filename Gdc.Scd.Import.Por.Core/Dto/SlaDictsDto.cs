using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public class SlaDictsDto
    {
        public Dictionary<string, long> Availability { get; set; }
        public Dictionary<string, long> ReactionTime { get; set; }
        public Dictionary<string, long> ReactionType { get; set; }
        public Dictionary<string, long> Locations { get; set; }
        public Dictionary<string, long> Duration { get; set; }
        public Dictionary<string, long> Proactive { get; set; }
    }
}
