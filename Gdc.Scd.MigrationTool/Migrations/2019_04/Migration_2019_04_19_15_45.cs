using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_19_15_45 : IMigrationAction
    {
        private readonly ICostBlockRepository costBlockRepository;

        public int Number => 83;

        public string Description => "Creating cost block region indexes";

        public Migration_2019_04_19_15_45(ICostBlockRepository costBlockRepository)
        {
            this.costBlockRepository = costBlockRepository;
        }

        public void Execute()
        {
            this.costBlockRepository.CreatRegionIndexes();
        }
    }
}
