using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_03_17_33 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 67;

        public string Description => "Alter [SpReleaseCosts] SP";

        public Migration_2019_04_03_17_33(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-03-17-33.sql");
        }
    }
}
