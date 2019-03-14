using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_13 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 39;

        public string Description => "Add Sog to HddRetentionView";

        public Migration_2019_03_13(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-13-19-12.sql");
        }
    }
}
