using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_14_15_45 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 101;

        public string Description => "Add locap global support approved report";

        public Migration_2019_05_14_15_45(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-14-15-45.sql");
        }
    }
}
