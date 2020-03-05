using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Impl;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_27_12_35 : AutoNumberMigrationAction
    {
        private readonly IDataMigrator dataMigrator;
        private readonly IMetaProvider metaProvider;
        private readonly IDomainService<Availability> availabilityService;

        private readonly ICostBlockRepository costBlockRepository;

        public override string Description => "Add dependency for cost element 'OohUpliftFactor' in 'FieldServiceCost' cost block";

        public Migration_2020_02_27_12_35(
            IDataMigrator dataMigrator, 
            IMetaProvider metaProvider, 
            IDomainService<Availability> availabilityService,
            ICostBlockRepository costBlockRepository)
        {
            this.dataMigrator = dataMigrator;
            this.metaProvider = metaProvider;
            this.availabilityService = availabilityService;
            this.costBlockRepository = costBlockRepository;
        }

        public override void Execute()
        {
            const string FieldServiceCost = "FieldServiceCost";

            var oldMeta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_02_27_12_35_old");
            var oldCostBlock = oldMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost];
            var availabilitity = this.availabilityService.GetAll().Where(item => item.Name == "24x7").First();
            var availabilitityField = oldCostBlock.DependencyFields["Availability"];

            availabilitityField.DefaultValue = availabilitity.Id;

            this.dataMigrator.AddColumn(oldCostBlock, availabilitityField.Name);

            var newMeta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_02_27_12_35_new");
            var newCostBlocks = newMeta.CostBlocks.GetSome(MetaConstants.HardwareSchema, FieldServiceCost);

            this.dataMigrator.SplitCostBlock(oldCostBlock, newCostBlocks, newMeta);
            //TODO: Not implemented 
            //this.dataMigrator.AddCalculatedColumn();

            this.costBlockRepository.UpdateByCoordinates(newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "OohUpliftFactor"]);
        }
    }
}
