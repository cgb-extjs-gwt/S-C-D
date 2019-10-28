using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_15_09_15 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 42;

        public string Description => "Change HDD retention reports, add SOG, update FSP codes";

        public Migration_2019_03_15_09_15(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-15-09-15.sql");
        }
    }
}
