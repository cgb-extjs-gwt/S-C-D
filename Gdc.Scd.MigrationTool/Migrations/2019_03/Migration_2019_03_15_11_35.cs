using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_15_11_35 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 44;

        public string Description => "Change hardware get cost, add sog";

        public Migration_2019_03_15_11_35(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-15-11-35.sql");
        }
    }
}
