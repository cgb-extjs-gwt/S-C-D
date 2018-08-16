using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostBlockFilterBuilder : ICostBlockFilterBuilder
    {
        private readonly DomainMeta meta;

        public CostBlockFilterBuilder(DomainMeta meta)
        {
            this.meta = meta;
        }

        public IDictionary<string, IEnumerable<object>> BuildRegionFilter(CostEditorContext context)
        {
            var filter = new Dictionary<string, IEnumerable<object>>();

            if (context.RegionInputId != null)
            {
                var costElement = this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId];

                filter.Add(costElement.RegionInput.Id, new object[] { context.RegionInputId });
            }

            return filter;
        }

        public IDictionary<string, IEnumerable<object>> BuildFilter(CostEditorContext context)
        {
            var filter = this.BuildRegionFilter(context);

            if (context.CostElementFilterIds != null)
            {
                var costElement = this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId];
                var filterValues = context.CostElementFilterIds.Cast<object>().ToArray();

                filter.Add(costElement.Dependency.Id, filterValues);
            }

            if (context.InputLevelFilterIds != null)
            {
                var costElement = this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId];
                var previousInputLevel = costElement.GetPreviousInputLevel(context.InputLevelId);

                if (previousInputLevel != null &&
                    (costElement.RegionInput == null || costElement.RegionInput.Id != previousInputLevel.Id))
                {
                    var filterValues = context.InputLevelFilterIds.Cast<object>().ToArray();

                    filter.Add(previousInputLevel.Id, filterValues);
                }
            }

            return filter;
        }
    }
}
