using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_10_04_17_53 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 123;

        public string Description => "Create temp tables for Fsp Codes";

        public Migration_2019_10_04_17_53(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-10-03-15-58.sql");
        }
    }
}
