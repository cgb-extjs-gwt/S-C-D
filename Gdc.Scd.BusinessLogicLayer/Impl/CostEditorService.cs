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

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        private readonly IQualityGateSevice qualityGateSevice;

        public CostEditorService(
            ICostEditorRepository costEditorRepository,
            ISqlRepository sqlRepository,
            ICostBlockHistoryService historySevice,
            IRepositorySet repositorySet,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            IQualityGateSevice qualityGateSevice,
            DomainMeta meta,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.costEditorRepository = costEditorRepository;
            this.sqlRepository = sqlRepository;
            this.historySevice = historySevice;
            this.meta = meta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.repositorySet = repositorySet;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
            this.qualityGateSevice = qualityGateSevice;
        }

        public async Task<IEnumerable<NamedId>> GetCostElementFilterItems(CostEditorContext context)
        {
            var costElement = this.meta.GetCostElement(context);

            return await this.GetCostElementFilterItems(context, costElement);
        }

        public async Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context)
        {
            var previousInputLevel =
                this.meta.GetCostElement(context)
                         .GetPreviousInputLevel(context.InputLevelId);

            var filter = this.costBlockFilterBuilder.BuildRegionFilter(context);

            return await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, previousInputLevel.Id, filter);
        }

        public async Task<IEnumerable<NamedId>> GetRegions(CostEditorContext context)
        {
            var costElement = this.meta.GetCostElement(context);

            return await this.GetRegions(context, costElement);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            var filter = this.costBlockFilterBuilder.BuildFilter(context);
            var editItemInfo = this.GetEditItemInfo(context);

            return await this.costEditorRepository.GetEditItems(editItemInfo, filter);
        }

        public async Task<IEnumerable<NamedId>> GetCostElementReferenceValues(CostEditorContext context)
        {
            IEnumerable<NamedId> referenceValues = null;

            var costBlock = this.domainEnitiesMeta.GetCostBlockEntityMeta(context);
            if (costBlock.CostElementsFields[context.CostElementId] is ReferenceFieldMeta field)
            {
                referenceValues = await this.sqlRepository.GetNameIdItems(field.ReferenceMeta, field.ReferenceValueField, field.ReferenceFaceField);
            }

            return referenceValues;
        }

        public async Task<CostElementData> GetCostElementData(CostEditorContext context)
        {
            var costElementMeta = this.meta.GetCostElement(context);

            return new CostElementData
            {
                Regions = await this.GetRegions(context, costElementMeta),
                Filters = await this.GetCostElementFilterItems(context, costElementMeta),
                ReferenceValues = await this.GetCostElementReferenceValues(context)
            };
        }

        public async Task<QualityGateResult> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context, ApprovalOption approvalOption)
        {
            QualityGateResult checkResult;

            if (approvalOption.IsApproving && !approvalOption.HasQualityGateErrors)
            {
                checkResult = await this.qualityGateSevice.Check(editItems, context);
            }
            else
            {
                checkResult = new QualityGateResult();
            }

            if (!checkResult.HasErrors)
            {
                var editItemInfo = this.GetEditItemInfo(context);
                var filter = this.costBlockFilterBuilder.BuildFilter(context);

                using (var transaction = this.repositorySet.GetTransaction())
                {
                    try
                    {
                        var result = await this.costEditorRepository.UpdateValues(editItems, editItemInfo, filter);

                        await this.historySevice.Save(context, editItems, approvalOption);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();

                        throw;
                    }
                }
            }

            return checkResult;
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
                var filter = this.costBlockFilterBuilder.BuildRegionFilter(context);

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
    }
}
