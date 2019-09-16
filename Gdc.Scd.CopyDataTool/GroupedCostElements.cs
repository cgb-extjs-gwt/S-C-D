using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.CopyDataTool
{
    public class GroupedCostElements
    {
        public List<string> CostElements { get; set; }
        public List<string> Coordinates { get; set; } 

        public GroupedCostElements()
        {
            CostElements = new List<string>();
            Coordinates = new List<string>();
        }
    }
}
