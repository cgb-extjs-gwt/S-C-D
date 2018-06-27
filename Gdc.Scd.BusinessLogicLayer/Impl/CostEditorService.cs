using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
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
            var filter = this.GetCountryFilter(context);
            var costElement = this.meta.GetCostBlock(context.CostBlockId).GetCostElement(context.CostElementId);

            return await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, costElement.Dependency.Id, filter);
        }

        public async Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context)
        {
            var previousInputLevel = this.meta.GetPreviousInputLevel(context.InputLevelId);
            var filter = this.GetCountryFilter(context);

            return await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, previousInputLevel.Id, filter);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            var filter = this.GetCountryFilter(context);

            if (context.CostElementFilterIds != null)
            {
                var costBlock = this.meta.GetCostBlock(context.CostBlockId);
                var costElement = costBlock.GetCostElement(context.CostElementId);

                filter.Add(costElement.Dependency.Id, context.CostElementFilterIds);
            }

            if (context.InputLevelFilterIds != null)
            {
                var previousInputLevel = this.meta.GetPreviousInputLevel(context.InputLevelId);

                filter.Add(previousInputLevel.Id, context.InputLevelFilterIds);
            }

            var editItemInfo = this.GetEditItemInfo(context);

            return await this.costEditorRepository.GetEditItems(editItemInfo, filter);
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context)
        {
            var editItemInfo = this.GetEditItemInfo(context);
                
            var lowerInputLevel = this.meta.InputLevels.Last();

            return await this.costEditorRepository.UpdateValues(editItems, editItemInfo);
        }

        private IDictionary<string, IEnumerable<object>> GetCountryFilter(CostEditorContext context)
        {
            var filter = new Dictionary<string, IEnumerable<object>>();

            if (!string.IsNullOrWhiteSpace(context.CountryId))
            {
                filter.Add(MetaConstants.CountryLevelId, new[] { context.CountryId });
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
