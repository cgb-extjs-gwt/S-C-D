using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_15_10_45 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 43;

        public string Description => "Change locap report, add multi wg select";

        public Migration_2019_03_15_10_45(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-15-10-45.sql");
        }
    }
}
