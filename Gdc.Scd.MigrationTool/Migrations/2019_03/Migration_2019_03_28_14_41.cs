using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_28_14_41 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 59;

        public string Description => "Fix calculation, add verify deactivated date time in cost block";

        public Migration_2019_03_28_14_41(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-28-14-41.sql");
        }
    }
}
