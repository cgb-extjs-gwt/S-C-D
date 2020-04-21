using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Impl;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_17_17_07 : AutoNumberMigrationAction
    {
        private readonly ICostBlockRepository costBlockRepository;
        private readonly IRepositorySet repositorySet;
        private readonly IMetaProvider metaProvider;

        public override string Description => "AvailabilityFee UpdateByCoordinates";

        public Migration_2020_04_17_17_07(
            ICostBlockRepository costBlockRepository,
            IRepositorySet repositorySet,
            IMetaProvider metaProvider)
        {
            this.costBlockRepository = costBlockRepository;
            this.repositorySet = repositorySet;
            this.metaProvider = metaProvider;
        }

        public override void Execute()
        {
            const string AvailabilityFeeWg = "AvailabilityFeeWg";

            this.repositorySet.ExecuteSql($@"
                ALTER TABLE [{MetaConstants.HardwareSchema}].[{AvailabilityFeeWg}] 
                ADD CONSTRAINT ModifiedDateTime_Default DEFAULT GETDATE() FOR {nameof(Core.Entities.AvailabilityFeeWg.ModifiedDateTime)}");

            var meta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_04_10_14_57_new");
            var costBlock = meta.CostBlocks[MetaConstants.HardwareSchema, AvailabilityFeeWg];

            this.costBlockRepository.UpdateByCoordinates(costBlock);
        }
    }
}
