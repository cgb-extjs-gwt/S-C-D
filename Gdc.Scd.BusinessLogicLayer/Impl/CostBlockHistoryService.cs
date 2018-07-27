using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //public CostEditHistory BuildHistory(CostEditorContext context, IEnumerable<EditItem> editItems)
        //{
        //    //return new CostEditHistory
        //    //{
        //    //    Edit = new HistoryActionInfo
        //    //    {
        //    //        DateTime = DateTime.UtcNow,
        //    //        User = this.userService.GetCurrentUser()
        //    //    },
        //    //    Context = new HistoryContext
        //    //    {
        //    //        ApplicationId = context.ApplicationId,
        //    //        RegionInputId = context.RegionInputId,
        //    //        CostBlockId = context.CostBlockId,
        //    //        CostElementId = context.CostElementId,
        //    //        InputLevelId = context.InputLevelId,
        //    //        CostElementFilterIds = this.GetSimpleValueList(context.CostElementFilterIds),
        //    //        InputLevelFilterIds = this.GetSimpleValueList(context.InputLevelFilterIds)
        //    //    },
        //    //    EditItems = editItems.Select(editItem => new HistoryEditItem
        //    //    {
        //    //        Name = editItem.Name,
        //    //        Value = (double)Convert.ChangeType(editItem.Value, TypeCode.Double)
        //    //    }).ToList()
        //    //};

        //    return new CostEditHistory
        //    {
        //        EditDate = DateTime.UtcNow,
        //        EditUser = this.userService.GetCurrentUser(),
        //        ApplicationId = context.ApplicationId,
        //        RegionInputId = context.RegionInputId,
        //        CostBlockId = context.CostBlockId,
        //        CostElementId = context.CostElementId,
        //        InputLevelId = context.InputLevelId,
        //        DependencyIds = this.GetIdEntities<Cost>(context.CostElementFilterIds),
        //        InputLevelIds = this.GetIdEntities(context.InputLevelFilterIds)
        //    }
        //}

        //private List<T> GetIdEntities<T>(IEnumerable<long> values) where T : SimpleValue<long>, new()
        //{
        //    return
        //        values == null
        //            ? null
        //            : values.Select(value => new T { Value = value })
        //                    .ToList();
        //}
    }
}
