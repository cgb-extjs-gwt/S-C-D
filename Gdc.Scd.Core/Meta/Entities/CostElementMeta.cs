using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseCostElementMeta<InputLevelMeta>
    {
        public InputType InputType { get; set; }

        public HashSet<string> TableViewRoles { get; set; }

        public HashSet<string> CostEditorRoles { get; set; }

        public InputLevelMeta GetPreviousInputLevel(string inputLevelId)
        {
            InputLevelMeta previousInputLevel = null;

            foreach (var inputLevel in this.InputLevels)
            {
                if (inputLevel.Id == inputLevelId)
                {
                    break;
                }

                previousInputLevel = inputLevel;
            }

            return previousInputLevel;
        }

        public IEnumerable<InputLevelMeta> FilterInputLevels(string maxInputLevelId)
        {
            foreach (var inputLevel in this.InputLevels.OrderBy(x => x.LevelNumber))
            {
                yield return inputLevel;

                if (inputLevel.Id == maxInputLevelId)
                {
                    break;
                }
            }
        }
    }
}
