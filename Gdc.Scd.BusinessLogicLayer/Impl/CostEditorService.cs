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

        public CostEditorService(ICostEditorRepository costEditorRepository, ISqlRepository sqlRepository)
        {
            this.costEditorRepository = costEditorRepository;
            this.sqlRepository = sqlRepository;
        }

        public async Task<IEnumerable<string>> GetCostElementFilterItems(DomainMeta meta, CostEditorContext context)
        {
            var costBlock = meta.GetCostBlock(context.CostBlockId);
            var costElement = costBlock.GetCostElement(context.CostElementId);
            var filter = this.GetCountryFilter(context);

            return await this.sqlRepository.GetDistinctValues(
                costElement.Dependency.Id,
                costBlock.Id, 
                context.ApplicationId,
                filter);
        }

        public async Task<IEnumerable<string>> GetInputLevelFilterItems(DomainMeta meta, CostEditorContext context)
        {
            var previousInputLevel = meta.GetPreviousInputLevel(context.InputLevelId);
            var filter = this.GetCountryFilter(context);

            return await this.sqlRepository.GetDistinctValues(
                previousInputLevel.Id,
                context.CostBlockId,
                context.ApplicationId,
                filter);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(DomainMeta meta, CostEditorContext context)
        {
            var filter = this.GetCountryFilter(context);

            if (context.CostElementFilterIds != null)
            {
                var costBlock = meta.GetCostBlock(context.CostBlockId);
                var costElement = costBlock.GetCostElement(context.CostElementId);

                filter.Add(costElement.Dependency.Id, context.CostElementFilterIds);
            }

            if (context.InputLevelFilterIds != null)
            {
                var previousInputLevel = meta.GetPreviousInputLevel(context.InputLevelId);

                filter.Add(previousInputLevel.Id, context.InputLevelFilterIds);
            }

            IEnumerable<EditItem> result;

            var editItemInfo = this.GetEditItemInfo(context);

            var lowerInputLevel = meta.InputLevels.Last();
            if (lowerInputLevel.Id == context.InputLevelId)
            {
                result = await this.costEditorRepository.GetEditItems(editItemInfo, filter);
            }
            else
            {
                result = await this.costEditorRepository.GetEditItemsByLevel(context.InputLevelId, editItemInfo, filter);
            }

            return result;
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, DomainMeta meta, CostEditorContext context)
        {
            int result;

            var editItemInfo = this.GetEditItemInfo(context);
                
            var lowerInputLevel = meta.InputLevels.Last();
            if (lowerInputLevel.Id == context.InputLevelId)
            {
                result = await this.costEditorRepository.UpdateValues(editItems, editItemInfo);
            }
            else
            {
                result = await this.costEditorRepository.UpdateValuesByLevel(editItems, editItemInfo, context.InputLevelId);
            }

            return result;
        }

        private IDictionary<string, IEnumerable<object>> GetCountryFilter(CostEditorContext context)
        {
            var filter = new Dictionary<string, IEnumerable<object>>();

            if (!string.IsNullOrWhiteSpace(context.CountryId))
            {
                filter.Add(InputLevelConstants.CountryLevelId, new[] { context.CountryId });
            }

            return filter;
        }

        private EditItemInfo GetEditItemInfo(CostEditorContext context)
        {
            return new EditItemInfo
            {
                SchemaName = context.ApplicationId,
                TableName = context.CostBlockId,
                NameColumn = context.InputLevelId,
                ValueColumn = context.CostElementId
            };
        }
    }
}
