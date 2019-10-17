using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_10_16_17_41 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 125;

        public string Description => "Optimize tables indexes";

        public Migration_2019_10_16_17_41(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-10-16-17-41.sql");
            this.repositorySet.ExecuteFromFile("2019-10-16-17-41_2.sql");
        }
    }
}
