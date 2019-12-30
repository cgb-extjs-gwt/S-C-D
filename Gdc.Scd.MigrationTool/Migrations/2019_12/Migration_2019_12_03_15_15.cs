using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_12_03_15_15 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 130;


        public string Description => "Fix release price, add release user id'";

        public Migration_2019_12_03_15_15(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-03-15-15.sql");
        }
    }
}
