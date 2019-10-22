using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_18_2 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 14;

        public string Description => "Fix LOCAP report, fix service TC";

        public Migration_2019_02_18_2(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-18-2.sql");
        }
    }
}
