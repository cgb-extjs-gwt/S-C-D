using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockMeta : BaseDomainMeta
    {
        public IEnumerable<string> ApplicationIds { get; set; }

        public MetaCollection<CostElementMeta> CostElements { get; set; }

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

        public QualityGate QualityGate { get; set; }
    }
}
