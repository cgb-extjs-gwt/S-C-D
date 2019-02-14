using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseCostElementMeta<InputLevelMeta>
    {
        public HashSet<string> TableViewRoles { get; set; }

        public HashSet<string> CostEditorRoles { get; set; }

        public ICollection<InputLevelMetaInfo<InputLevelMeta>> InputLevelMetaInfos { get; set; }

        public override IEnumerable<InputLevelMeta> InputLevels => this.InputLevelMetaInfos.Select(inputLevelInfo => inputLevelInfo.InputLevel);

        public IEnumerable<InputLevelMeta> SortInputLevel()
        {
            return this.InputLevels.OrderBy(inputLevel => inputLevel.LevelNumber);
        }

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
            return this.GetFilterInputLevel(inputLevelId) != null;
        }
    }
}
