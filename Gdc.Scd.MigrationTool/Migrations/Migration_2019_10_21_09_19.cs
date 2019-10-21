using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_10_21_09_19 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 126;

        public string Description => "Optimize tables indexes";

        public Migration_2019_10_21_09_19(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-10-21-09-19.sql");
            this.repositorySet.ExecuteFromFile("2019-10-21-09-19_2.sql");
        }
    }
}
