using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_20_17_05 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 51;

        public string Description => "Add WG Hardware filter";

        public Migration_2019_03_20_17_05(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-20-17-05.sql");
        }
    }
}
