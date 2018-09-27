using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Dto.Import
{
    public class SlaDto
    {
        public long? ReactionTime { get; set; }
        public long? ReactionType { get; set; }
        public long? Duration { get; set; }
        public long? Availability { get; set; }
        public long? ServiceLocation { get; set; }
        public long? ProActive { get; set; }
    }
}
