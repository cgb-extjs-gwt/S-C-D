using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostBlockService : ICostBlockService
    {
        private readonly ICostBlockRepository costBlockRepository;

        private readonly IRepositorySet repositorySet;

        private readonly IUserService userService;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        private readonly ISqlRepository sqlRepository;

        private readonly DomainEnitiesMeta meta;

        public CostBlockService(
            ICostBlockRepository costBlockRepository, 
            IRepositorySet repositorySet,
            IUserService userService,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            ISqlRepository sqlRepository,
            DomainEnitiesMeta meta)
        {
            this.costBlockRepository = costBlockRepository;
            this.repositorySet = repositorySet;
            this.userService = userService;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
            this.sqlRepository = sqlRepository;      
            this.meta = meta;
        }

        public async Task UpdateByCoordinatesAsync(
            IEnumerable<CostBlockEntityMeta> costBlockMetas,
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    foreach (var costBlockMeta in costBlockMetas)
                    {
                        await this.costBlockRepository.UpdateByCoordinatesAsync(costBlockMeta, updateOptions);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }
        }

        public void UpdateByCoordinates(
            IEnumerable<CostBlockEntityMeta> costBlockMetas, 
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            foreach (var costBlockMeta in costBlockMetas)
            {
                this.costBlockRepository.UpdateByCoordinates(costBlockMeta, updateOptions);
            }
        }

        public async Task UpdateByCoordinatesAsync(IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            await this.UpdateByCoordinatesAsync(this.meta.CostBlocks, updateOptions);
        }

        public void UpdateByCoordinates(IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            this.UpdateByCoordinates(this.meta.CostBlocks, updateOptions);
        }

        public async Task<IEnumerable<NamedId>> GetCoordinateItems(HistoryContext context, string coordinateId)
        {
            var meta = this.meta.GetCostBlockEntityMeta(context);
            var referenceField = 
                meta.GetDomainCoordinateFields(context.CostElementId)
                    .First(field => field.Name == coordinateId);

            var userCountries = this.userService.GetCurrentUserCountries();
            var costBlockFilter = this.costBlockFilterBuilder.BuildRegionFilter(context, userCountries).Convert();
            var referenceFilter = this.costBlockFilterBuilder.BuildCoordinateItemsFilter(referenceField.ReferenceMeta);
            var notDeletedCondition = CostBlockQueryHelper.BuildNotDeletedCondition(meta);

            return 
                await this.sqlRepository.GetDistinctItems(
                    meta, 
                    referenceField.Name, 
                    costBlockFilter, 
                    referenceFilter,
                    notDeletedCondition);
        }

        public async Task<IEnumerable<NamedId>> GetDependencyItems(HistoryContext context)
        {
            IEnumerable<NamedId> filterItems = null;

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.DomainMeta.CostElements[context.CostElementId];

            if (costElementMeta.Dependency != null)
            {
                filterItems = await this.GetCoordinateItems(context, costElementMeta.Dependency.Id);
            }

            return filterItems;
        }
    }
}
