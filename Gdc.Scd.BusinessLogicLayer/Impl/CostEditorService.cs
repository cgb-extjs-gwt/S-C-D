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

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ICostBlockHistoryService historySevice;

        private readonly IRepositorySet repositorySet;

        public CostEditorService(
            ICostEditorRepository costEditorRepository,
            ISqlRepository sqlRepository,
            ICostBlockHistoryService historySevice,
            IRepositorySet repositorySet,
            DomainMeta meta,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.costEditorRepository = costEditorRepository;
            this.sqlRepository = sqlRepository;
            this.historySevice = historySevice;
            this.meta = meta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.repositorySet = repositorySet;
        }

        public async Task<IEnumerable<NamedId>> GetCostElementFilterItems(CostEditorContext context)
        {
            var costElement = this.GetCostElementMeta(context);

            return await this.GetCostElementFilterItems(context, costElement);
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
            var costElement = this.GetCostElementMeta(context);

            return await this.GetRegions(context, costElement);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            var filter = this.GetFilter(context);
            var editItemInfo = this.GetEditItemInfo(context);

            return await this.costEditorRepository.GetEditItems(editItemInfo, filter);
        }

        public async Task<IEnumerable<NamedId>> GetCostElementReferenceValues(CostEditorContext context)
        {
            IEnumerable<NamedId> referenceValues = null;

            var costBlock = (CostBlockEntityMeta)this.domainEnitiesMeta.GetEntityMeta(context.CostBlockId, context.ApplicationId);
            if (costBlock.CostElementsFields[context.CostElementId] is ReferenceFieldMeta field)
            {
                referenceValues = await this.sqlRepository.GetNameIdItems(field.ReferenceMeta, field.ReferenceValueField, field.ReferenceFaceField);
            }

            return referenceValues;
        }

        public async Task<CostElementData> GetCostElementData(CostEditorContext context)
        {
            var costElementMeta = this.GetCostElementMeta(context);

            return new CostElementData
            {
                Regions = await this.GetRegions(context, costElementMeta),
                Filters = await this.GetCostElementFilterItems(context, costElementMeta),
                ReferenceValues = await this.GetCostElementReferenceValues(context)
            };
        }

        public async Task<int> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context)
        {
            var editItemInfo = this.GetEditItemInfo(context);
            var filter = this.GetFilter(context);

            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    var result = await this.costEditorRepository.UpdateValues(editItems, editItemInfo, filter);

                    await this.historySevice.Save(context, editItems);

                    transaction.Commit();

                    return result;
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }
        }

        private IDictionary<string, IEnumerable<object>> GetRegionFilter(CostEditorContext context)
        {
            var filter = new Dictionary<string, IEnumerable<object>>();

            if (context.RegionInputId != null)
            {
                var costElement = this.GetCostElementMeta(context);

                filter.Add(costElement.RegionInput.Id, new object[] { context.RegionInputId });
            }

            return filter;
        }

        private IDictionary<string, IEnumerable<object>> GetFilter(CostEditorContext context)
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

        private async Task<IEnumerable<NamedId>> GetCostElementFilterItems(CostEditorContext context, CostElementMeta costElementMeta)
        {
            IEnumerable<NamedId> filterItems = null;

            if (costElementMeta.Dependency != null)
            {
                var filter = this.GetRegionFilter(context);

                filterItems = await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, costElementMeta.Dependency.Id, filter);
            }

            return filterItems;
        }

        private async Task<IEnumerable<NamedId>> GetRegions(CostEditorContext context, CostElementMeta costElementMeta)
        {
            IEnumerable<NamedId> regions = null;

            if (costElementMeta.RegionInput != null)
            {
                regions = await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, costElementMeta.RegionInput.Id);
            }

            return regions;
        }

        private CostElementMeta GetCostElementMeta(CostEditorContext context)
        {
            return this.meta.CostBlocks[context.CostBlockId].CostElements[context.CostElementId];
        }
    }
}
