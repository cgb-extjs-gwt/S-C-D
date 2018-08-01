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

        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(CostBlockHistory history)
        {
            return await this.costBlockValueHistoryRepository.GetByCostBlockHistory(history);
        }

        public async Task<IEnumerable<CostBlockValueHistory>> GetHistoryValues(long costBlockHistoryId)
        {
            var history = this.repositorySet.GetRepository<CostBlockHistory>().Get(costBlockHistoryId);

            return await this.GetHistoryValues(history);
        }

        public async Task Save(CostEditorContext context, IEnumerable<EditItem> editItems)
        {
            var history = new CostBlockHistory
            {
                EditDate = DateTime.UtcNow,
                EditUser = this.userService.GetCurrentUser(),
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

            if (costElementMeta.Dependency != null && 
                context.CostElementFilterIds != null && 
                context.CostElementFilterIds.Length > 0)
            {
                relatedItems.Add(costElementMeta.Dependency.Name, context.CostElementFilterIds);
            }

            var inputLevelMeta = costElementMeta.GetPreviousInputLevel(context.InputLevelId);
            if (inputLevelMeta != null && 
                context.InputLevelFilterIds != null && 
                context.InputLevelFilterIds.Length > 0)
            {
                relatedItems.Add(inputLevelMeta.Name, context.InputLevelFilterIds);
            }

            await this.costBlockValueHistoryRepository.Save(history, editItems, relatedItems);
        }
    }
}
