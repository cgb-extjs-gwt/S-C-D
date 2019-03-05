using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_05_09_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 30;

        public string Description => "Fix report sw, add filter by sog";

        public Migration_2019_03_05_09_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-05-09-00.sql");
        }
    }
}
