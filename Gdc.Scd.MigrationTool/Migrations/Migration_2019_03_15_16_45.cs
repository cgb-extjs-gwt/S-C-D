using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_15_16_45 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 45;

        public string Description => "Change hardware get cost, add sog";

        public Migration_2019_03_15_16_45(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-15-16-45.sql");
        }
    }
}
