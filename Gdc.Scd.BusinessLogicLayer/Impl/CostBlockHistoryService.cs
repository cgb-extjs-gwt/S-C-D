using System;
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
    public class CostBlockHistoryService : ICostBlockHistoryService
    {
        private readonly IUserService userService;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockValueHistoryRepository costBlockValueHistoryRepository;

        private readonly DomainMeta meta;

        public CostBlockHistoryService(
            IRepositorySet repositorySet,
            IUserService userService,
            ICostBlockValueHistoryRepository costBlockValueHistoryRepository,
            DomainMeta meta)
        {
            this.userService = userService;
            this.repositorySet = repositorySet;
            this.costBlockValueHistoryRepository = costBlockValueHistoryRepository;
            this.meta = meta;
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

        public void Reject(long historyId)
        {
            var historyRepository = this.repositorySet.GetRepository<CostBlockHistory>();
            var history = historyRepository.Get(historyId);

            this.SetHistoryState(history, CostBlockHistoryState.Rejected);

            historyRepository.Save(history);

            this.repositorySet.Sync();
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(CostBlockHistory history)
        {
            return await this.costBlockValueHistoryRepository.GetByCostBlockHistory(history);
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(long costBlockHistoryId)
        {
            var history = this.repositorySet.GetRepository<CostBlockHistory>().Get(costBlockHistoryId);

            return await this.GetHistoryValues(history);
        }

        public async Task Save(CostEditorContext context, IEnumerable<EditItem> editItems, bool forApproval)
        {
            var history = new CostBlockHistory
            {
                EditDate = DateTime.UtcNow,
                EditUser = this.userService.GetCurrentUser(),
                State = forApproval ? CostBlockHistoryState.Pending : CostBlockHistoryState.None,
                Context = new HistoryContext
                {
                    ApplicationId = context.ApplicationId,
                    RegionInputId = context.RegionInputId,
                    CostBlockId = context.CostBlockId,
                    CostElementId = context.CostElementId,
                    InputLevelId = context.InputLevelId,
                }
            };

            var costBlockHistoryRepository = this.repositorySet.GetRepository<CostBlockHistory>();

            costBlockHistoryRepository.Save(history);
            this.repositorySet.Sync();

            var relatedItems = new Dictionary<string, long[]>();

            var costBlockMeta = this.meta.CostBlocks[context.CostBlockId];
            var costElementMeta = costBlockMeta.CostElements[context.CostElementId];

            if (costElementMeta.RegionInput != null &&
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

            await this.costBlockValueHistoryRepository.Save(history, editItems, relatedItems);
        }

        private void SetHistoryState(CostBlockHistory history, CostBlockHistoryState state)
        {
            history.ApproveRejectDate = DateTime.UtcNow;
            history.ApproveRejectUser = this.userService.GetCurrentUser();
            history.State = state;
        }

        private IQueryable<CostBlockHistory> FilterHistories(IQueryable<CostBlockHistory> query, CostBlockHistoryFilter filter)
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

            return query;
        }
    }
}
