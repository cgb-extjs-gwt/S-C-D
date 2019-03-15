using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_18_12_25 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 13;

        public string Description => "Add country folders for CD CS";

        public Migration_2019_02_18_12_25(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-18-12-25.sql");
        }
    }
}
