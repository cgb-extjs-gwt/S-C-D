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

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostBlockHistoryService : ReadingDomainService<CostBlockHistory>, ICostBlockHistoryService
    {
        private readonly IUserService userService;

        private readonly ICostBlockValueHistoryRepository costBlockValueHistoryRepository;

        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        public CostBlockHistoryService(
            IRepositorySet repositorySet,
            IUserService userService,
            ICostBlockValueHistoryRepository costBlockValueHistoryRepository,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            DomainMeta domainMeta,
            DomainEnitiesMeta domainEnitiesMeta)
            : base(repositorySet)
        {
            this.userService = userService;
            this.costBlockValueHistoryRepository = costBlockValueHistoryRepository;
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
        }

        public IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryFilter filter)
        {
            return this.FilterHistories(this.GetAll(), filter);
        }

        public IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryState state)
        {
            return this.GetAll().Where(history => history.State == state);
        }

        public IQueryable<CostBlockHistory> GetByFilter(CostBlockHistoryFilter filter, CostBlockHistoryState state)
        {
            return this.FilterHistories(this.GetByFilter(state), filter);
        }

        public async Task<IEnumerable<HistoryItem>> GetHistoryItems(CostEditorContext context, long editItemId, QueryInfo queryInfo = null)
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

        public async Task<IEnumerable<HistoryItem>> GetHistoryItems(CostElementIdentifier costElementId, IDictionary<string, long> coordinates, QueryInfo queryInfo = null)
        {
            var historyContext = new HistoryContext
            {
                ApplicationId = costElementId.ApplicationId,
                CostBlockId = costElementId.CostBlockId,
                CostElementId = costElementId.CostElementId
            };

            var costBlockMeta = this.domainEnitiesMeta.GetCostBlockEntityMeta(historyContext);
            var inputLevels = this.GetInputLevels(costBlockMeta);
            var coordinateInfo = this.BuildCoordinateInfo(coordinates, inputLevels);

            historyContext.InputLevelId = coordinateInfo.InputLevel.Id;

            var filter = coordinateInfo.Filter.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

            return await this.costBlockValueHistoryRepository.GetHistory(historyContext, filter, queryInfo);
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
                var inputLevelMetas = this.GetInputLevels(editInfo.Meta);

                var costElementGroups =
                    editInfo.ValueInfos.Select(info => new
                                        {
                                            CostElementValues = info.Values,
                                            CoordinateIfno = this.BuildCoordinateInfo(info.Coordinates, inputLevelMetas)
                                        })
                                       .SelectMany(info => info.CostElementValues.Select(costElemenValue => new
                                       {
                                           CostElementValue = costElemenValue,
                                           info.CoordinateIfno.Filter,
                                           info.CoordinateIfno.InputLevel
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
                                filterGroup.Select(info => new EditItem { Id = info.InputLevel.Value, Value = info.CostElementValue.Value })
                                           .ToArray();

                            var filter = filterGroup.Key == null
                                ? new Dictionary<string, long[]>()
                                : filterGroup.Key.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

                            await this.Save(context, editItems, approvalOption, filter);
                        }
                    }
                }
            }
        }

        public void Save(CostBlockHistory history, ApprovalOption approvalOption)
        {
            if (approvalOption.HasQualityGateErrors && string.IsNullOrWhiteSpace(approvalOption.QualityGateErrorExplanation))
            {
                throw new Exception("QualityGateErrorExplanation must be");
            }

            history.State = approvalOption.IsApproving ? CostBlockHistoryState.Approving : CostBlockHistoryState.Saved;

            this.repositorySet.GetRepository<CostBlockHistory>().Save(history);
            this.repositorySet.Sync();
        }

        public CostBlockHistory SaveAsApproved(long historyId)
        {
            var history = this.Get(historyId);

            this.SetState(history, CostBlockHistoryState.Approved);

            this.repository.Save(history);
            this.repositorySet.Sync();

            return history;
        }

        public CostBlockHistory SaveAsRejected(long historyId, string rejectedMessage)
        {
            var history = this.Get(historyId);

            this.SetState(history, CostBlockHistoryState.Rejected);

            this.repository.Save(history);
            this.repositorySet.Sync();

            return history;
        }

        private void SetState(CostBlockHistory history, CostBlockHistoryState state)
        {
            history.ApproveRejectDate = DateTime.UtcNow;
            history.ApproveRejectUser = this.userService.GetCurrentUser();
            history.State = state;
        }

        private IDictionary<string, InputLevelMeta> GetInputLevels(CostBlockEntityMeta meta)
        {
            return meta.DomainMeta.InputLevels.ToDictionary(inputLevel => inputLevel.Id);
        }

        private (IDictionary<string, long> Filter, (string Id, long Value) InputLevel) BuildCoordinateInfo(
            IDictionary<string, long> coordinates, 
            IDictionary<string, InputLevelMeta> inputLevelMetas)
        {
            InputLevelMeta maxInputLevelMeta = null;
            (string, long)? inputLevel = null;

            foreach (var coordinate in coordinates)
            {
                var inputLevelMeta = inputLevelMetas[coordinate.Key];

                if (inputLevelMeta != null &&
                    (maxInputLevelMeta == null || maxInputLevelMeta.LevelNumber < inputLevelMeta.LevelNumber))
                {
                    maxInputLevelMeta = inputLevelMeta;
                    inputLevel = (coordinate.Key, coordinate.Value);
                }
            }

            var filter = new Dictionary<string, long>(coordinates);

            filter.Remove(maxInputLevelMeta.Id);

            return (filter, inputLevel.Value);
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
