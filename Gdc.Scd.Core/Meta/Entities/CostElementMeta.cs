using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostElementMeta : BaseCostElementMeta<InputLevelMeta>
    {
        public HashSet<string> TableViewRoles { get; set; }

        public HashSet<string> CostEditorRoles { get; set; }

        public MetaCollection<InputLevelMetaInfo<InputLevelMeta>> InputLevelMetaInfos { get; set; }

        public bool IncludeDisabledDependcyItems { get; set; }

        public override IEnumerable<InputLevelMeta> InputLevels => this.InputLevelMetaInfos.Select(inputLevelInfo => inputLevelInfo.InputLevel);

        public IEnumerable<BaseMeta> Coordinates
        {
            get
            {
                foreach (var inputLevel in this.InputLevels)
                {
                    yield return inputLevel;
                }

                if (this.RegionInput != null && !this.InputLevelMetaInfos.Contains(this.RegionInput.Id))
                {
                    yield return this.RegionInput;
                }

                if (this.Dependency != null)
                {
                    yield return this.Dependency;
                }
            }
        }

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

        public bool HasInputLevelFilter(string inputLevelId)
        {
            return this.GetFilterInputLevel(inputLevelId) != null;
        }

        public InputLevelMeta GetInputLevel(string inputLevelId)
        {
            return this.InputLevelMetaInfos[inputLevelId]?.InputLevel;
        }

        public bool HasInputLevel(string inputLevelId)
        {
            return this.InputLevelMetaInfos.Contains(inputLevelId);
        }
    }
}
