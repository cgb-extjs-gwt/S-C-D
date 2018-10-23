using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockMeta : BaseCostBlockMeta<CostElementMeta>
    {
        public IEnumerable<InputLevelMeta> InputLevels
        {
            get
            {
                return
                    this.CostElements.SelectMany(costElement => costElement.InputLevels)
                                     .Distinct()
                                     .OrderBy(inputLevel => inputLevel.LevelNumber);
            }
        }
    }
}
