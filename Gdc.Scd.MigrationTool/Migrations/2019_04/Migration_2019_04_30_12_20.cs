using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_30_12_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 94;

        public string Description => "Fix HW overview report, fix availability fee view in currency";

        public Migration_2019_04_30_12_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-30-12-20.sql");
        }
    }
}
