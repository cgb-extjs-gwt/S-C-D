using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    public class SlaDto
    {
        public string FspCode { get; set; }
        public string ServiceLocation { get; set; }
        public string Availability { get; set; }
        public string ReactionTime { get; set; }
        public string ReactionType { get; set; }
        public string WarrantyGroup { get; set; }
        public string Duration { get; set; }
    }
}
