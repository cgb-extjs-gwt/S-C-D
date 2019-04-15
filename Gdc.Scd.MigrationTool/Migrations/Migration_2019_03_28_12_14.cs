using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_28_12_14 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 58;

        public string Description => "Create [Hardware].[SpReleaseCosts] SP";

        public Migration_2019_03_28_12_14(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-28-12-14.sql");
        }
    }
}
