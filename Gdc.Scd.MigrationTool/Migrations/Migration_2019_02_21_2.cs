using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_21_2 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 16;

        public string Description => "Fix typo in Transfer Price";

        public Migration_2019_02_21_2(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("Migration_2019_02_21_2.sql");
        }
    }
}
