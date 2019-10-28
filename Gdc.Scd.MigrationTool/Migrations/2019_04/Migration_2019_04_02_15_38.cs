using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_02_15_38 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 64;

        public string Description => "Add archive cost block procedures";

        public Migration_2019_04_02_15_38(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-02-15-38.sql");
        }
    }
}
