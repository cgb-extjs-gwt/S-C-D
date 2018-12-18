using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseCostElementMeta<InputLevelMeta>
    {
        public HashSet<string> TableViewRoles { get; set; }

        public HashSet<string> CostEditorRoles { get; set; }

        public IEnumerable<InputLevelMeta> SortInputLevel()
        {
            return this.InputLevels.OrderBy(inputLevel => inputLevel.LevelNumber);
        }

        public InputLevelMeta GetPreviousInputLevel(string inputLevelId)
        {
            InputLevelMeta previousInputLevel = null;

            foreach (var inputLevel in this.SortInputLevel())
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
            foreach (var inputLevel in this.SortInputLevel())
            {
                yield return inputLevel;

                if (inputLevel.Id == maxInputLevelId)
                {
                    break;
                }
            }
        }

        public bool HasInputLevelFilter(string inputLevelId)
        {
            var prevInputLevel = this.GetPreviousInputLevel(inputLevelId);

            return this.HasInputLevelFilter(prevInputLevel);
        }

        public bool HasInputLevelFilter(InputLevelMeta prevInputLevel)
        {
            return prevInputLevel != null && this.RegionInput != prevInputLevel;
        }
    }
}
