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
        public List<string> InputLevels { get; set; } //Правильней это переименовать в Coordinates, там помимо InputLevels могут быть и Dependency

        public GroupedCostElements()
        {
            CostElements = new List<string>();
            InputLevels = new List<string>();
        }
    }
}
