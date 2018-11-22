using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostBlockService : ICostBlockService
    {
        private readonly ICostBlockRepository costBlockRepository;

        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta meta;

        public CostBlockService(ICostBlockRepository costBlockRepository, IRepositorySet repositorySet, DomainEnitiesMeta meta)
        {
            this.costBlockRepository = costBlockRepository;
            this.repositorySet = repositorySet;
            this.meta = meta;
        }

        public async Task UpdateByCoordinatesAsync(IEnumerable<CostBlockEntityMeta> costBlockMetas)
        {
            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    foreach (var costBlockMeta in costBlockMetas)
                    {
                        await this.costBlockRepository.UpdateByCoordinatesAsync(costBlockMeta);
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

        public void UpdateByCoordinates(IEnumerable<CostBlockEntityMeta> costBlockMetas)
        {
            foreach (var costBlockMeta in costBlockMetas)
            {
                this.costBlockRepository.UpdateByCoordinates(costBlockMeta);
                
            }
        }

        public async Task UpdateByCoordinatesAsync()
        {
            await this.UpdateByCoordinatesAsync(this.meta.CostBlocks);
        }

        public void UpdateByCoordinates()
        {
            this.UpdateByCoordinates(this.meta.CostBlocks);
        }
    }
}
