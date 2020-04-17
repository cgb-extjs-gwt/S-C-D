using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Impl;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_17_17_07 : AutoNumberMigrationAction
    {
        private readonly DomainEnitiesMeta meta;
        private readonly ICostBlockRepository costBlockRepository;
        private readonly IRepositorySet repositorySet;

        public override string Description => "AvailabilityFee UpdateByCoordinates";

        public Migration_2020_04_17_17_07(            
            DomainEnitiesMeta meta,
            ICostBlockRepository costBlockRepository,
            IRepositorySet repositorySet)
        {
            this.meta = meta;
            this.costBlockRepository = costBlockRepository;
            this.repositorySet = repositorySet;
        }

        public override void Execute()
        {
            this.repositorySet.ExecuteSql($@"
                ALTER TABLE [{MetaConstants.HardwareSchema}].[{MetaConstants.AvailabilityFeeWgCostBlock}] 
                ADD CONSTRAINT ModifiedDateTime_Default DEFAULT GETDATE() FOR {nameof(AvailabilityFeeWg.ModifiedDateTime)}");

            var costBlock = this.meta.CostBlocks[MetaConstants.HardwareSchema, MetaConstants.AvailabilityFeeCountryWgCostBlock];

            this.costBlockRepository.UpdateByCoordinates(costBlock);
        }
    }
}
