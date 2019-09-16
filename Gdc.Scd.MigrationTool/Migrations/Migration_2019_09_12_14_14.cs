using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_09_12_14_14 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 119;

        public string Description => "Add new service location";

        public Migration_2019_09_12_14_14(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-09-12-14-14.sql");
        }
    }
}
