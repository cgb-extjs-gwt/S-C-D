using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_20_12_05 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 46;

        public string Description => "Change locap report, remove columns";

        public Migration_2019_03_20_12_05(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-20-12-05.sql");
        }
    }
}
