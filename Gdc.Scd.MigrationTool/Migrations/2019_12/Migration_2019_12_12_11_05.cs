using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_12_12_11_05 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 132;

        public string Description => "Fix contract report";

        public Migration_2019_12_12_11_05(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-12-11-05.sql");
        }
    }
}
