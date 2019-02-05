using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
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

        private readonly IDomainService<Country> countryService;

        public CostEditorService(
            ICostEditorRepository costEditorRepository,
            ISqlRepository sqlRepository,
            ICostBlockHistoryService historySevice,
            IRepositorySet repositorySet,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            IQualityGateSevice qualityGateSevice,
            IUserService userService,
            ICostBlockService costBlockService,
            IDomainService<Country> countryService,
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
            this.countryService = countryService;
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

        public async Task<CostEditorDto> GetCostElementData(CostEditorContext context)
        {
            var dependencyItems = await this.costBlockService.GetDependencyItems(context);

            return new CostEditorDto
            {
                Filters = dependencyItems,
                Regions = await this.GetRegions(context),
                ReferenceValues = await this.GetCostElementReferenceValues(context),
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

        public async Task<IEnumerable<HistoryItemDto>> GetHistoryItems(CostEditorContext context, long editItemId, QueryInfo queryInfo = null)
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

        private async Task<RegionDto[]> GetRegions(CostEditorContext context)
        {
            RegionDto[] regionDtos = null;

            var regions = await this.costBlockService.GetRegions(context);
            if (regions != null)
            {
                var costElement = this.meta.GetCostElement(context);
                if (costElement.IsCountryCurrencyCost)
                {
                    var countryIds = regions.Select(region => region.Id).ToArray();

                    regionDtos =
                        this.countryService.GetAll()
                                           .Where(country => countryIds.Contains(country.Id))
                                           .Select(country => new RegionDto(country, country.Currency))
                                           .ToArray();
                }
                else
                {
                    regionDtos = regions.Select(region => new RegionDto(region)).ToArray();
                }
            }

            return regionDtos;
        }
    }
}
