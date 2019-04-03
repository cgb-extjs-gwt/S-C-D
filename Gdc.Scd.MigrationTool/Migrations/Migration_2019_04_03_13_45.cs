using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_03_13_45 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 66;

        public string Description => "Add locap report columns";

        public Migration_2019_04_03_13_45(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-03-13-45.sql");
        }
    }
}
