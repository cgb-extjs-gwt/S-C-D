using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_06_15_15 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 32;

        public string Description => "Change markup to local currency";

        public Migration_2019_03_06_15_15(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-06-15-25.sql");
        }
    }
}
