using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_10_09_12_57 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 124;

        public string Description => "Fix CD CS, add new columns to service cost sheet";

        public Migration_2019_10_09_12_57(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-10-09-12-57.sql");
        }
    }
}
