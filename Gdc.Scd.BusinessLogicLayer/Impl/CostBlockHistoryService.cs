using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Web.BusinessLogicLayer.Entities;

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

        private readonly IQualityGateSevice qualityGateSevice;

        public CostBlockHistoryService(
            IRepositorySet repositorySet,
            IUserService userService,
            ICostBlockValueHistoryRepository costBlockValueHistoryRepository,
            ISqlRepository sqlRepository,
            IEmailService emailService,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            IQualityGateRepository qualityGateRepository,
            IQualityGateSevice qualityGateSevice,
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
            this.qualityGateSevice = qualityGateSevice;
        }

        public IQueryable<CostBlockHistory> GetHistories()
        {
            return this.repositorySet.GetRepository<CostBlockHistory>().GetAll();
        }

        public IQueryable<CostBlockHistory> GetHistories(CostBlockHistoryFilter filter)
        {
            return this.FilterHistories(this.GetHistories(), filter);
        }

        public IQueryable<CostBlockHistory> GetHistories(CostBlockHistoryState state)
        {
            return this.GetHistories().Where(history => history.State == state);
        }

        public IQueryable<CostBlockHistory> GetHistories(CostBlockHistoryFilter filter, CostBlockHistoryState state)
        {
            return this.FilterHistories(this.GetHistories(state), filter);
        }

        public async Task<IEnumerable<ApprovalBundle>> GetApprovalBundles(CostBlockHistoryFilter filter, CostBlockHistoryState state)
        {
            var histories = this.GetHistories(filter, state).ToArray();
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
                    RegionInput = regionInput,
                    QualityGateErrorExplanation = history.QualityGateErrorExplanation
                };

                historyDtos.Add(historyDto);
            }

            return historyDtos;
        }

        public async Task<IEnumerable<HistoryItem>> GetHistory(CostEditorContext context, long editItemId, QueryInfo queryInfo = null)
        {
            var historyContext = HistoryContext.Build(context);
            var userCountries = this.userService.GetCurrentUserCountries();
            var filter = this.costBlockFilterBuilder.BuildFilter(context, userCountries);
            var region = this.domainMeta.GetCostElement(context).RegionInput;

            if (region == null || region.Id != context.InputLevelId)
            {
                filter.Add(context.InputLevelId, new long[] { editItemId });
            }

            return await this.costBlockValueHistoryRepository.GetHistory(historyContext, filter, queryInfo);
        }

        public async Task<QualityGateResultDto> SendForApproval(long historyId, string qualityGateErrorExplanation = null)
        {
            var historyRepository = this.repositorySet.GetRepository<CostBlockHistory>();
            var history = historyRepository.Get(historyId);

            QualityGateResultDto qualityGateResult;

            var option = new ApprovalOption
            {
                IsApproving = true,
                QualityGateErrorExplanation = qualityGateErrorExplanation
            };

            if (string.IsNullOrWhiteSpace(qualityGateErrorExplanation))
            {
                qualityGateResult = await this.qualityGateSevice.CheckAsQualityGateResultDto(history);
            }
            else
            {
                qualityGateResult = new QualityGateResultDto();
            }

            if (!qualityGateResult.HasErrors)
            {
                this.Save(history, option);
            }

            return qualityGateResult;
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

        public async Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetail(
            CostBlockHistory history, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            IEnumerable<CostBlockValueHistory> result;

            if (this.qualityGateSevice.IsUseCheck(history.Context))
            {
                result = await this.qualityGateRepository.GetApproveBundleDetailQualityGate(history, historyValueId, costBlockFilter);
            }
            else
            {
                result = await this.costBlockValueHistoryRepository.GetApproveBundleDetail(history, historyValueId, costBlockFilter);
            }

            return result;
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetApproveBundleDetail(
            long costBlockHistoryId, 
            long? historyValueId = null, 
            IDictionary<string, IEnumerable<object>> costBlockFilter = null)
        {
            var history = this.repositorySet.GetRepository<CostBlockHistory>().Get(costBlockHistoryId);

            return await this.GetApproveBundleDetail(history, historyValueId, costBlockFilter);
        }

        public async Task Save(CostEditorContext context, IEnumerable<EditItem> editItems, ApprovalOption approvalOption, IDictionary<string, long[]> filter)
        {
            if (approvalOption.HasQualityGateErrors && string.IsNullOrWhiteSpace(approvalOption.QualityGateErrorExplanation))
            {
                throw new Exception("QualityGateErrorExplanation must be");
            }

            var editItemArray = editItems.ToArray();
            var isDifferentValues = 
                editItemArray.Length > 0 && 
                editItemArray.All(item => item.Value == editItemArray[0].Value);

            var history = new CostBlockHistory
            {
                EditDate = DateTime.UtcNow,
                EditUser = this.userService.GetCurrentUser(),
                Context = HistoryContext.Build(context),
                EditItemCount = editItemArray.Length,
                IsDifferentValues = isDifferentValues, 
                HasQualityGateErrors = approvalOption.HasQualityGateErrors,
                QualityGateErrorExplanation = approvalOption.QualityGateErrorExplanation
            };

            this.Save(history, approvalOption);

            var relatedItems = new Dictionary<string, long[]>(filter)
            {
                [context.InputLevelId] = editItems.Select(item => item.Id).ToArray()
            };

            await this.costBlockValueHistoryRepository.Save(history, editItemArray, relatedItems);
        }

        public async Task Save(IEnumerable<EditInfo> editInfos, ApprovalOption approvalOption)
        {
            foreach (var editInfo in editInfos)
            {
                var inputLevelMetas = editInfo.Meta.DomainMeta.InputLevels.ToDictionary(inputLevel => inputLevel.Id);

                var costElementGroups =
                    editInfo.ValueInfos.Select(info => GetInputLevelInfo(info, inputLevelMetas))
                                       .SelectMany(info => info.CostElementValues.Select(costElemenValue => new
                                       {
                                           CostElementValue = costElemenValue,
                                           info.Filter,
                                           info.InputLevel
                                       }))
                                       .GroupBy(info => info.CostElementValue.Key);

                foreach (var costElementGroup in costElementGroups)
                {
                    foreach (var inputLevelGroup in costElementGroup.GroupBy(info => info.InputLevel.Id))
                    {
                        var context = new CostEditorContext
                        {
                            ApplicationId = editInfo.Meta.ApplicationId,
                            CostBlockId = editInfo.Meta.CostBlockId,
                            InputLevelId = inputLevelGroup.Key,
                            CostElementId = costElementGroup.Key
                        };

                        var filterGroups = inputLevelGroup.GroupBy(info => info.Filter.Count == 0 ? null : info.Filter);

                        foreach (var filterGroup in filterGroups)
                        {
                            var editItems =
                                filterGroup.Select(info => new EditItem { Id = info.InputLevel.Value, Value = info.CostElementValue })
                                           .ToArray();

                            var filter = filterGroup.Key == null
                                ? new Dictionary<string, long[]>()
                                : filterGroup.Key.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

                            await this.Save(context, editItems, approvalOption, filter);
                        }
                    }
                }
            }

            (IDictionary<string, object> CostElementValues, IDictionary<string, long> Filter, (string Id, long Value) InputLevel) GetInputLevelInfo(
                ValuesInfo valuesInfo,
                IDictionary<string, InputLevelMeta> inputLevelMetas)
            {
                InputLevelMeta maxInputLevelMeta = null;
                (string, long)? inputLevel = null;

                var filter = new Dictionary<string, long>();

                foreach (var coordinate in valuesInfo.Coordinates)
                {
                    var inputLevelMeta = inputLevelMetas[coordinate.Key];

                    if (inputLevelMeta != null && 
                        (maxInputLevelMeta == null || maxInputLevelMeta.LevelNumber < inputLevelMeta.LevelNumber))
                    {
                            maxInputLevelMeta = inputLevelMeta;
                            inputLevel = (coordinate.Key, coordinate.Value);
                    }

                    filter.Add(coordinate.Key, coordinate.Value);
                }

                return (valuesInfo.Values, filter, inputLevel.Value);
            }
        }

        private void Save(CostBlockHistory history, ApprovalOption approvalOption)
        {
            if (approvalOption.HasQualityGateErrors && string.IsNullOrWhiteSpace(approvalOption.QualityGateErrorExplanation))
            {
                throw new Exception("QualityGateErrorExplanation must be");
            }

            history.State = approvalOption.IsApproving ? CostBlockHistoryState.Approving : CostBlockHistoryState.Saved;

            this.repositorySet.GetRepository<CostBlockHistory>().Save(history);
            this.repositorySet.Sync();
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
