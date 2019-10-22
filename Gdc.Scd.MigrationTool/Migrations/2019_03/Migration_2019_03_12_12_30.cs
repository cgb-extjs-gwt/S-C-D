using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_12_12_30 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 38;

        public string Description => "Fix report sw overview column heading";

        public Migration_2019_03_12_12_30(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-12-12-30.sql");
        }
    }
}
