using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_02_16_23 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 65;

        public string Description => "Fix contract report";

        public Migration_2019_04_02_16_23(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-02-16-23.sql");
        }
    }
}
