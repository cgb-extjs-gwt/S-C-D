using System;
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
    public class CostBlockHistoryService : ICostBlockHistoryService
    {
        private readonly IUserService userService;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockValueHistoryRepository costBlockValueHistoryRepository;

        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ISqlRepository sqlRepository;

        private readonly IEmailService emailService;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        private readonly IQualityGateRepository qualityGateRepository;

        public CostBlockHistoryService(
            IRepositorySet repositorySet,
            IUserService userService,
            ICostBlockValueHistoryRepository costBlockValueHistoryRepository,
            ISqlRepository sqlRepository,
            IEmailService emailService,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            IQualityGateRepository qualityGateRepository,
            DomainMeta domainMeta,
            DomainEnitiesMeta domainEnitiesMeta)
        {
            this.userService = userService;
            this.repositorySet = repositorySet;
            this.costBlockValueHistoryRepository = costBlockValueHistoryRepository;
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.sqlRepository = sqlRepository;
            this.emailService = emailService;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
            this.qualityGateRepository = qualityGateRepository;
        }

        public IQueryable<CostBlockHistory> GetHistories()
        {
            return this.repositorySet.GetRepository<CostBlockHistory>().GetAll();
        }

        public IQueryable<CostBlockHistory> GetHistories(CostBlockHistoryFilter filter)
        {
            return this.FilterHistories(this.GetHistories(), filter);
        }

        public IQueryable<CostBlockHistory> GetHistoriesForApproval()
        {
            return this.GetHistories().Where(history => history.State == CostBlockHistoryState.Pending);
        }

        public IQueryable<CostBlockHistory> GetHistoriesForApproval(CostBlockHistoryFilter filter)
        {
            return this.FilterHistories(this.GetHistoriesForApproval(), filter);
        }

        public async Task<IEnumerable<ApprovalBundle>> GetApprovalBundles(CostBlockHistoryFilter filter)
        {
            var histories = this.GetHistoriesForApproval(filter).ToArray();
            var historyInfos =
                histories.Select(history => new
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

            var historyDtos = new List<ApprovalBundle>();

            foreach (var history in histories)
            {
                var costBlock = this.domainMeta.CostBlocks[history.Context.CostBlockId];
                var costElement = costBlock.CostElements[history.Context.CostElementId];
                var regionInput = costElement.RegionInput == null 
                    ? null 
                    : regionCache[costElement.RegionInput][history.Context.RegionInputId.Value];

                var historyDto = new ApprovalBundle
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
                    InputLevel = MetaDto.Build(costElement.InputLevels[history.Context.InputLevelId]),
                    RegionInput = regionInput
                };

                historyDtos.Add(historyDto);
            }

            return historyDtos;
        }

        public async Task<IEnumerable<HistoryItem>> GetHistory(CostEditorContext context, long editItemId, QueryInfo queryInfo = null)
        {
            var historyContext = HistoryContext.Build(context);
            var filter = this.costBlockFilterBuilder.BuildFilter(context);
            var region = this.domainMeta.GetCostElement(context).RegionInput;

            if (region == null || region.Id != context.InputLevelId)
            {
                filter.Add(context.InputLevelId, new object[] { editItemId });
            }

            return await this.costBlockValueHistoryRepository.GetHistory(historyContext, filter, queryInfo);
        }

        public async Task Approve(long historyId)
        {
            var historyRepository = this.repositorySet.GetRepository<CostBlockHistory>();
            var history = historyRepository.Get(historyId);

            this.SetHistoryState(history, CostBlockHistoryState.Approved);

            historyRepository.Save(history);

            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    this.repositorySet.Sync();

                    await this.costBlockValueHistoryRepository.Approve(history);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }
        }

        public void Reject(long historyId, string message = null)
        {
            var historyRepository = this.repositorySet.GetRepository<CostBlockHistory>();
            var history = historyRepository.Get(historyId);

            this.SetHistoryState(history, CostBlockHistoryState.Rejected);

            history.RejectMessage = message;

            historyRepository.Save(history);

            this.repositorySet.Sync();

            if (message != null)
            {
                this.emailService.SendEmail(history.EditUser, "SCD 2.0", message);
            }
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetail(CostBlockHistory history, long? historyValueId = null)
        {
            IEnumerable<CostBlockValueHistory> result;

            if (history.HasQualityGateErrors && historyValueId.HasValue)
            {
                result = await this.qualityGateRepository.GetApproveBundleDetailQualityGate(history, historyValueId.Value);
            }
            else
            {
                result = await this.costBlockValueHistoryRepository.GetApproveBundleDetail(history, historyValueId);
            }

            return result;
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetail(long costBlockHistoryId, long? historyValueId = null)
        {
            var history = this.repositorySet.GetRepository<CostBlockHistory>().Get(costBlockHistoryId);

            return await this.GetApproveBundleDetail(history, historyValueId);
        }

        public async Task Save(CostEditorContext context, IEnumerable<EditItem> editItems, ApprovalOption approvalOption)
        {
            if (approvalOption.HasQualityGateErrors && string.IsNullOrWhiteSpace(approvalOption.QualityGateErrorExplanation))
            {
                throw new Exception("QualityGateErrorExplanation must be");
            }

            var editItemArray = editItems.ToArray();
            var isDifferentValues = false;

            if (editItemArray.Length > 0)
            {
                isDifferentValues = editItemArray.All(item => item.Value == editItemArray[0].Value);
            }

            var history = new CostBlockHistory
            {
                EditDate = DateTime.UtcNow,
                EditUser = this.userService.GetCurrentUser(),
                State = approvalOption.IsApproving ? CostBlockHistoryState.Pending : CostBlockHistoryState.None,
                Context = HistoryContext.Build(context),
                EditItemCount = editItemArray.Length,
                IsDifferentValues = isDifferentValues, 
                HasQualityGateErrors = approvalOption.HasQualityGateErrors,
                QualityGateErrorExplanation = approvalOption.QualityGateErrorExplanation
            };

            var costBlockHistoryRepository = this.repositorySet.GetRepository<CostBlockHistory>();

            costBlockHistoryRepository.Save(history);
            this.repositorySet.Sync();

            var relatedItems = new Dictionary<string, long[]>
            {
                [context.InputLevelId] = editItems.Select(item => item.Id).ToArray()
            };

            var costBlockMeta = this.domainMeta.CostBlocks[context.CostBlockId];
            var costElementMeta = costBlockMeta.CostElements[context.CostElementId];

            if (costElementMeta.RegionInput != null &&
                costElementMeta.RegionInput.Id != context.InputLevelId &&
                context.RegionInputId != null)
            {
                relatedItems.Add(costElementMeta.RegionInput.Id, new[] { context.RegionInputId.Value });
            }

            if (costElementMeta.Dependency != null && 
                context.CostElementFilterIds != null && 
                context.CostElementFilterIds.Length > 0)
            {
                relatedItems.Add(costElementMeta.Dependency.Id, context.CostElementFilterIds);
            }

            var inputLevelMeta = costElementMeta.GetPreviousInputLevel(context.InputLevelId);
            if (inputLevelMeta != null && 
                context.InputLevelFilterIds != null && 
                context.InputLevelFilterIds.Length > 0)
            {
                relatedItems.Add(inputLevelMeta.Id, context.InputLevelFilterIds);
            }

            await this.costBlockValueHistoryRepository.Save(history, editItemArray, relatedItems);
        }

        private void SetHistoryState(CostBlockHistory history, CostBlockHistoryState state)
        {
            history.ApproveRejectDate = DateTime.UtcNow;
            history.ApproveRejectUser = this.userService.GetCurrentUser();
            history.State = state;
        }

        private IQueryable<CostBlockHistory> FilterHistories(IQueryable<CostBlockHistory> query, CostBlockHistoryFilter filter)
        {
            if (filter != null)
            {
                if (filter.DateTimeFrom.HasValue)
                {
                    query = query.Where(history => filter.DateTimeFrom.Value <= history.EditDate);
                }

                if (filter.DateTimeTo.HasValue)
                {
                    query = query.Where(history => history.EditDate <= filter.DateTimeTo);
                }

                if (filter.ApplicationIds != null && filter.ApplicationIds.Length > 0)
                {
                    query = query.Where(history => filter.ApplicationIds.Contains(history.Context.ApplicationId));
                }

                if (filter.CostBlockIds != null && filter.CostBlockIds.Length > 0)
                {
                    query = query.Where(history => filter.CostBlockIds.Contains(history.Context.CostBlockId));
                }

                if (filter.CostElementIds != null && filter.CostElementIds.Length > 0)
                {
                    query = query.Where(history => filter.CostElementIds.Contains(history.Context.CostElementId));
                }

                if (filter.UserIds != null && filter.UserIds.Length > 0)
                {
                    query = query.Where(history => filter.UserIds.Contains(history.EditUser.Id));
                }
            }

            return query;
        }
    }
}
