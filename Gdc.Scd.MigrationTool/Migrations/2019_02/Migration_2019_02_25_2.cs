using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_25_2 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 23;

        public string Description => "Locap report, optimize reporting and portfolio";

        public Migration_2019_02_25_2(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-25-1.sql");
            repositorySet.ExecuteFromFile("2019-02-25-2.sql");
            repositorySet.ExecuteFromFile("2019-02-25-3.sql");
        }
    }
}
