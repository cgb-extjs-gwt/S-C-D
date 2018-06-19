using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Meta.Constants;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;

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

        public async Task<IEnumerable<string>> GetInputLevelFilterItems(CostEditorContext context)
        {
            var filter = this.GetCountryFilter(context);

            return await this.sqlRepository.GetDistinctValues(
                context.InputLevelId,
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
                filter.Add(context.InputLevelId, context.InputLevelFilterIds);
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

        public void UpdateValues(IEnumerable<EditItem> editItems, DomainMeta meta, CostEditorContext context)
        {
            var editItemInfo = this.GetEditItemInfo(context);
                
            var lowerInputLevel = meta.InputLevels.Last();
            if (lowerInputLevel.Id == context.InputLevelId)
            {
                this.costEditorRepository.UpdateValues(editItems, editItemInfo);
            }
            else
            {
                this.costEditorRepository.UpdateValuesByLevel(editItems, editItemInfo, context.InputLevelId);
            }
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
