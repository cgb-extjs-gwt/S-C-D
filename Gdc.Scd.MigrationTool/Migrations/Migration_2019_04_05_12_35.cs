using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_05_12_35 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 70;

        public string Description => "Fix hdd retention calc report";

        public Migration_2019_04_05_12_35(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-05-12-35.sql");
        }
    }
}
