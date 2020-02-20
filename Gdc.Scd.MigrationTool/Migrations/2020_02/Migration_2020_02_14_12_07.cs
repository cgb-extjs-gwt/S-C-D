using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_14_12_07 : IMigrationAction
    {
        private readonly ICostBlockRepository costBlockRepository;
        private readonly DomainEnitiesMeta meta;

        public int Number => 77777;

        public string Description => "Add cost element 'OohUpliftFactor' in 'FieldServiceCost' cost block";

        public Migration_2020_02_14_12_07(ICostBlockRepository costBlockRepository, DomainEnitiesMeta meta)
        {
            this.costBlockRepository = costBlockRepository;
            this.meta = meta;
        }

        public void Execute()
        {
            this.costBlockRepository.AddCostBlocks(new[]
            {
                this.meta.CostBlocks[MetaConstants.HardwareSchema, "FieldServiceCost", "OohUpliftFactor"]
            });
        }
    }
}
