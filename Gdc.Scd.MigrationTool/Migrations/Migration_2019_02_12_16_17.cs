using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_12_16_17 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 3;

        public string Description => "Hdd retention schema change. Add List price/dealer price. Fix hdd retention reports";

        public Migration_2019_02_12_16_17(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-12-16-17.sql");
        }
    }
}
