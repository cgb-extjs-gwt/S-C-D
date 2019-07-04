using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_07_04_10_05 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 112;

        public string Description => "Fix Locap reports, remove positive ";

        public Migration_2019_07_04_10_05(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-07-04-10-05.sql");
        }
    }
}
