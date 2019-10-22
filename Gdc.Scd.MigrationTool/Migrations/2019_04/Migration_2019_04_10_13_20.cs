using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_10_13_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 75;

        public string Description => "Fix calc parameter hw report";

        public Migration_2019_04_10_13_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-10-13-20.sql");
        }
    }
}
