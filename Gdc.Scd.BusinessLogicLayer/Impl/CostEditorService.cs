using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
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

        private readonly IUserService userService;

        private readonly ICostBlockService costBlockService;

        public CostEditorService(
            ICostEditorRepository costEditorRepository,
            ISqlRepository sqlRepository,
            ICostBlockHistoryService historySevice,
            IRepositorySet repositorySet,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            IQualityGateSevice qualityGateSevice,
            IUserService userService,
            ICostBlockService costBlockService,
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
            this.userService = userService;
            this.costBlockService = costBlockService;
        }

        public async Task<IEnumerable<NamedId>> GetInputLevelFilterItems(CostEditorContext context)
        {
            var previousInputLevel =
                this.meta.GetCostElement(context)
                         .GetFilterInputLevel(context.InputLevelId);

            return await this.costBlockService.GetCoordinateItems(context, previousInputLevel.Id);
        }

        public async Task<IEnumerable<EditItem>> GetEditItems(CostEditorContext context)
        {
            IEnumerable<EditItem> editItems;

            if (HasVisibleData())
            {
                var userCountries = this.userService.GetCurrentUserCountries();
                var filter = this.costBlockFilterBuilder.BuildFilter(context, userCountries);

                editItems = await this.costEditorRepository.GetEditItems(context, filter);
            }
            else
            {
                editItems = Enumerable.Empty<EditItem>();
            }

            return editItems;

            bool HasVisibleData()
            {
                var costElement = this.meta.GetCostElement(context);

                return
                    (costElement.Dependency == null || (context.CostElementFilterIds != null && context.CostElementFilterIds.Length != 0)) &&
                    (!costElement.HasInputLevelFilter(context.InputLevelId) || (context.InputLevelFilterIds != null && context.InputLevelFilterIds.Length != 0));
            }
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
            var userCountries = this.userService.GetCurrentUserCountries().ToArray();

            return new CostElementData
            {
                Regions = await this.GetRegions(context, costElementMeta, userCountries),
                Filters = await this.costBlockService.GetDependencyItems(context),
                ReferenceValues = await this.GetCostElementReferenceValues(context)
            };
        }

        public async Task<QualityGateResult> UpdateValues(IEnumerable<EditItem> editItems, CostEditorContext context, ApprovalOption approvalOption)
        {
            var userCountries = this.userService.GetCurrentUserCountries();
            var filter = this.costBlockFilterBuilder.BuildFilter(context, userCountries);
            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(context);

            var editInfos = new[]
            {
                new EditInfo
                {
                    Meta = costBlockMeta,
                    ValueInfos = editItems.Select(editItem => new ValuesInfo
                    {
                        CoordinateFilter = new Dictionary<string, long[]>(filter)
                        {
                            [context.InputLevelId] = new [] { editItem.Id }
                        },
                        Values = new Dictionary<string, object>
                        {
                            [context.CostElementId] = editItem.Value
                        }
                    }).ToArray()
                }
            };

            var qualityGateResultSet = await this.costBlockService.Update(editInfos, approvalOption, EditorType.CostEditor);
            var qualityGateResultSetItem = qualityGateResultSet.Items.FirstOrDefault();

            return 
                qualityGateResultSetItem == null 
                    ? new QualityGateResult() 
                    : qualityGateResultSetItem.QualityGateResult;
        }

        public async Task<IEnumerable<HistoryItem>> GetHistoryItems(CostEditorContext context, long editItemId, QueryInfo queryInfo = null)
        {
            var userCountries = this.userService.GetCurrentUserCountries();
            var filter = this.costBlockFilterBuilder.BuildFilter(context, userCountries);
            var region = this.meta.GetCostElement(context).RegionInput;

            if (region == null || region.Id != context.InputLevelId)
            {
                filter.Add(context.InputLevelId, new long[] { editItemId });
            }

            return await this.historySevice.GetHistoryItems(context, filter, queryInfo);
        }

        private async Task<IEnumerable<NamedId>> GetRegions(CostEditorContext context, CostElementMeta costElementMeta, Country[] userCountries)
        {
            IEnumerable<NamedId> regions = null;

            if (costElementMeta.RegionInput != null)
            {
                if (userCountries.Length == 0)
                {
                    regions = await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, costElementMeta.RegionInput.Id);
                }
                else
                {
                    regions = userCountries.Select(country => new NamedId { Id = country.Id, Name = country.Name }).ToArray();
                }
            }

            return regions;
        }
    }
}
