using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_06_10_09_23 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 110;

        public string Description => "Fix SW solution reports";

        public Migration_2019_06_10_09_23(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-06-10-09-23.sql");
        }
    }
}
