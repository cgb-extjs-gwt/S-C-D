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

        public IEnumerable<BaseMeta> Coordinates => this.CostElements.SelectMany(costElement => costElement.Coordinates).Distinct();

        public InputLevelMeta GetMaxInputLevel(IEnumerable<string> inputLevelIds)
        {
            var inputLevelMetas = this.InputLevels.ToDictionary(inputLevel => inputLevel.Id);

            return
                inputLevelIds.Where(coordinateId => inputLevelMetas.ContainsKey(coordinateId))
                             .Select(coordinateId => inputLevelMetas[coordinateId])
                             .OrderByDescending(inputLevel => inputLevel.LevelNumber)
                             .FirstOrDefault();
        }
    }
}
