using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_20_15_25 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 48;

        public string Description => "Add portfolio history";

        public Migration_2019_03_20_15_25(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-20-15-25.sql");
        }
    }
}
