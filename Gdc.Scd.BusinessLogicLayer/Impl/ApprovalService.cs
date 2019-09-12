using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Approval;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ApprovalService : IApprovalService
    {
        private readonly ICostBlockHistoryService costBlockHistoryService;

        private readonly ISqlRepository sqlRepository;

        private readonly IRepositorySet repositorySet;

        private readonly IQualityGateSevice qualityGateSevice;

        private readonly IApprovalRepository approvalRepository;

        private readonly IEmailService emailService;

        private readonly IUserService userService;

        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        public ApprovalService(
            ICostBlockHistoryService costBlockHistoryService, 
            ISqlRepository sqlRepository,
            IRepositorySet repositorySet,
            IQualityGateRepository qualityGateRepository,
            IQualityGateSevice qualityGateSevice,
            IApprovalRepository approvalRepository,
            IEmailService emailService,
            IUserService userService,
            DomainMeta domainMeta, 
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.costBlockHistoryService = costBlockHistoryService;
            this.sqlRepository = sqlRepository;
            this.repositorySet = repositorySet;
            this.qualityGateSevice = qualityGateSevice;
            this.approvalRepository = approvalRepository;
            this.emailService = emailService;
            this.userService = userService;
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
        }

        public async Task<IEnumerable<BundleDto>> GetApprovalBundles(BundleFilter filter)
        {
            var histories = this.costBlockHistoryService.GetByFilter(filter);

            if (filter.State == CostBlockHistoryState.Approved || filter.State == CostBlockHistoryState.Rejected)
            {
                var user = this.userService.GetCurrentUser();

                if (!user.Permissions.Any(permission => permission.Name == PermissionConstants.ApprovalShowAllItems))
                {
                    histories = histories.Where(history => history.ApproveRejectUser.Id == user.Id);
                }
            }

            return await this.GetApprovalBundles(histories.ToArray());
        }

        public async Task<IEnumerable<BundleDto>> GetOwnApprovalBundles(BundleFilter filter)
        {
            var user = this.userService.GetCurrentUser();
            var histories = 
                this.costBlockHistoryService.GetByFilter(filter)
                                            .Where(history => history.EditUser.Id == user.Id)
                                            .ToArray();

            return await this.GetApprovalBundles(histories);
        }

        public async Task<QualityGateResult> SendForApproval(long historyId, string qualityGateErrorExplanation = null)
        {
            var history = this.costBlockHistoryService.Get(historyId);

            QualityGateResult qualityGateResult;

            var option = new ApprovalOption
            {
                IsApproving = true,
                QualityGateErrorExplanation = qualityGateErrorExplanation
            };

            if (string.IsNullOrWhiteSpace(qualityGateErrorExplanation))
            {
                qualityGateResult = await this.qualityGateSevice.Check(history);
            }
            else
            {
                qualityGateResult = new QualityGateResult();
            }

            if (!qualityGateResult.HasErrors)
            {
                this.costBlockHistoryService.Save(history, option);
            }

            return qualityGateResult;
        }

        public async Task Approve(long historyId, bool turnOffNotification = false)
        {
            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    var history = this.costBlockHistoryService.SaveAsApproved(historyId);

                    await this.approvalRepository.Approve(history);

                    transaction.Commit();

                    try
                    {
                        if (!turnOffNotification)
                            await this.emailService.SendApprovalMailAsync(history);
                    }
                    catch
                    {
                        // TODO: Need to add logging
                    }
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }
        }

        public async Task Reject(long historyId, string message = null, bool turnOffNotification = false)
        {
            var history = this.costBlockHistoryService.SaveAsRejected(historyId, message);

            if (message != null && !turnOffNotification)
            {
                await this.emailService.SendRejectedMailAsync(history, message, userService.GetCurrentUser().Name);
            }
        }

        public async Task<IEnumerable<BundleDetailGroupDto>> GetApproveBundleDetails(
            CostBlockHistory history,
            long? historyValueId = null,
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            IEnumerable<BundleDetail> bundleDetails;

            var qulityGateOption = this.qualityGateSevice.GetQualityGateOption(history.Context, history.EditorType);

            if (history.State == CostBlockHistoryState.Approving && this.qualityGateSevice.IsUseCheck(qulityGateOption))
            {
                bundleDetails = await this.approvalRepository.GetApproveBundleDetailQualityGate(history, qulityGateOption.IsCountryCheck, historyValueId, costBlockFilter);
            }
            else
            {
                bundleDetails = await this.approvalRepository.GetApproveBundleDetail(history, historyValueId, costBlockFilter);
            }

            return bundleDetails.ToBundleDetailGroups();
        }

        public async Task<IEnumerable<BundleDetailGroupDto>> GetApproveBundleDetails(
            long costBlockHistoryId,
            long? historyValueId = null,
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var history = this.costBlockHistoryService.Get(costBlockHistoryId);

            return await this.GetApproveBundleDetails(history, historyValueId, costBlockFilter);
        }

        private async Task<IEnumerable<BundleDto>> GetApprovalBundles(CostBlockHistory[] histories)
        {

            var userCountriesIds = this.userService.GetCurrentUserCountries().Select(country => country.Id).ToArray();
            if (userCountriesIds.Length > 0)
            {
                histories = histories.Where(history =>
                    history.Context.RegionInputId.HasValue &&
                    userCountriesIds.Contains(history.Context.RegionInputId.Value)).ToArray();
            }
            var historyInfos =
                histories
                    .Select(history => new
                    {
                        History = history,
                        RegionInput = this.domainMeta.GetCostElement(history.Context).RegionInput
                    });
            
            
            var historyInfoGroups = historyInfos.Where(info => info.RegionInput != null).GroupBy(info => info.RegionInput);

            var regionCache = new Dictionary<InputLevelMeta, Dictionary<long, NamedId>>();

            foreach (var historyInfoGroup in historyInfoGroups)
            {
                var regionIds =
                    historyInfoGroup.Where(historyInfo => historyInfo.History.Context.RegionInputId.HasValue)
                                    .Select(historyInfo => historyInfo.History.Context.RegionInputId.Value);

                var entityMeta = this.domainEnitiesMeta.GetInputLevel(historyInfoGroup.Key.Id);
                var regions = await this.sqlRepository.GetNameIdItems(entityMeta, entityMeta.IdField.Name, entityMeta.NameField.Name, regionIds);

                regionCache.Add(historyInfoGroup.Key, regions.ToDictionary(region => region.Id));
            }

            var historyDtos = new List<BundleDto>();

            foreach (var history in histories)
            {
                var costBlock = this.domainMeta.CostBlocks[history.Context.CostBlockId];
                var costElement = costBlock.CostElements[history.Context.CostElementId];
                var regionInput = costElement.RegionInput == null || !history.Context.RegionInputId.HasValue
                    ? null
                    : regionCache[costElement.RegionInput][history.Context.RegionInputId.Value];

                var inputLevel = costElement.GetInputLevel(history.Context.InputLevelId);

                var historyDto = new BundleDto
                {
                    Id = history.Id,
                    EditDate = history.EditDate,
                    EditUser = new NamedId
                    {
                        Id = history.EditUser.Id,
                        Name = history.EditUser.Name
                    },
                    EditItemCount = history.EditItemCount,
                    IsDifferentValues = history.IsDifferentValues,
                    Application = MetaDto.Build(this.domainMeta.Applications[history.Context.ApplicationId]),
                    CostBlock = MetaDto.Build(costBlock),
                    CostElement = MetaDto.Build(costElement),
                    InputLevel = MetaDto.Build(inputLevel),
                    RegionInput = regionInput,
                    State = history.State,
                    QualityGateErrorExplanation = history.QualityGateErrorExplanation
                };

                historyDtos.Add(historyDto);
            }

            return historyDtos;
        }
    }
}
