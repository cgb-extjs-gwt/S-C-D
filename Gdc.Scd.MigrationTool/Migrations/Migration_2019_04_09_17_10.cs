using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_09_17_10 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 75;

        public string Description => "Add new locap reports with approved values";

        public Migration_2019_04_09_17_10(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-09-17-10.sql");
        }
    }
}
