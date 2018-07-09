using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostEditorService : ICostEditorService
    {
        private readonly ICostEditorRepository costEditorRepository;

        private readonly ISqlRepository sqlRepository;

        private readonly DomainMeta meta;

        public CostEditorService(ICostEditorRepository costEditorRepository, ISqlRepository sqlRepository, DomainMeta meta)
        {
            this.costEditorRepository = costEditorRepository;
            this.sqlRepository = sqlRepository;
            this.meta = meta;
        }

        public async Task<IEnumerable<NamedId>> GetCostElementFilterItems(CostEditorContext context)
        {
            var filter = this.GetRegionFilter(context);
            var costElement = this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId];

            return await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, costElement.Dependency.Id, filter);
        }

        public async Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context)
        {
            var previousInputLevel = 
                this.meta.CostBlocks[context.CostBlockId]
                         .CostElements[context.CostElementId]
                         .GetPreviousInputLevel(context.InputLevelId);

            var filter = this.GetRegionFilter(context);

            return await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, previousInputLevel.Id, filter);
        }

        public async Task<IEnumerable<NamedId>> GetRegions(CostEditorContext context)
        {
            var regionInput = this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId].RegionInput;

            return await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, regionInput.Id);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            var filter = this.GetRegionFilter(context);

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
                var filterValues = context.InputLevelFilterIds.Cast<object>().ToArray();

                filter.Add(previousInputLevel.Id, filterValues);
            }

            var editItemInfo = this.GetEditItemInfo(context);

            return await this.costEditorRepository.GetEditItems(editItemInfo, filter);
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context)
        {
            var editItemInfo = this.GetEditItemInfo(context);
                
            return await this.costEditorRepository.UpdateValues(editItems, editItemInfo);
        }

        private IDictionary<string, IEnumerable<object>> GetRegionFilter(CostEditorContext context)
        {
            var filter = new Dictionary<string, IEnumerable<object>>();

            if (context.RegionInputId.HasValue)
            {
                var regionInput = this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId].RegionInput;

                filter.Add(regionInput.Id, new object[] { context.RegionInputId.Value });
            }

            return filter;
        }

        private EditItemInfo GetEditItemInfo(CostEditorContext context)
        {
            return new EditItemInfo
            {
                Schema = context.ApplicationId,
                EntityName = context.CostBlockId,
                NameField = context.InputLevelId,
                ValueField = context.CostElementId
            };
        }
    }
}
