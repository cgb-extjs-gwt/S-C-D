using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_06_12_46 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 96;

        public string Description => "Fix SW parameter overview report, change SW licence";

        public Migration_2019_05_06_12_46(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-06-12-46.sql");
        }
    }
}
