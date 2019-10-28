using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_17_18_57 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 80;

        public string Description => "Release yearly costs";

        public Migration_2019_04_17_18_57(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-17-18-57.sql");
        }
    }
}
