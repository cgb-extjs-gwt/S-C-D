using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_16_09_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 77;

        public string Description => "Fix calc parameter hw report. Change column name";

        public Migration_2019_04_16_09_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-16-09-00.sql");
        }
    }
}
