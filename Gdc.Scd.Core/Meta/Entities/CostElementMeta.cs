using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseCostElementMeta<InputLevelMeta>
    {
        public InputType InputType { get; set; }

        public HashSet<string> TableViewRoles { get; set; }

        public HashSet<string> CostEditorRoles { get; set; }

        public IEnumerable<InputLevelMeta> SortInputLevel()
        {
            return this.InputLevels.OrderBy(inputLevel => inputLevel.LevelNumber);
        }

        //public InputLevelMeta GetPreviousInputLevel(string inputLevelId)
        //{
        //    InputLevelMeta previousInputLevel = null;

        //    foreach (var inputLevel in this.SortInputLevel().Where(il => !il.HideFilter))
        //    {
        //        if (inputLevel.Id == inputLevelId)
        //        {
        //            break;
        //        }

        //        previousInputLevel = inputLevel;
        //    }

        //    return previousInputLevel;
        //}

        public InputLevelMeta GetFilterInputLevel(string inputLevelId)
        {
            InputLevelMeta previousInputLevel = null;
            var currentFilter = this.SortInputLevel().FirstOrDefault(f => f.Id == inputLevelId);
            if (currentFilter == null || currentFilter.HideFilter)
                return null;
            
            foreach (var inputLevel in this.SortInputLevel().Where(f => !f.HideFilter))
            {
                if (inputLevel.Id == inputLevelId)
                {
                    if (inputLevel.HideFilter)
                        return null;

                    break;
                }

                previousInputLevel = inputLevel;
            }

            if (previousInputLevel != null && previousInputLevel == this.RegionInput)
                return null;

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
            var prevInputLevel = this.GetFilterInputLevel(inputLevelId);

            return this.HasInputLevelFilter(prevInputLevel);
        }

        public bool HasInputLevelFilter(InputLevelMeta prevInputLevel)
        {
            return prevInputLevel != null;
        }
    }
}
