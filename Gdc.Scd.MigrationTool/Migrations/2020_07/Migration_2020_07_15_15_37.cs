using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_07_15_15_37 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public string Description => "Fix Std material report, fix Hw overview report";

        public int Number => 188;

        public Migration_2020_07_15_15_37(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-07-15-15-37.sql");
        }
    }
}
