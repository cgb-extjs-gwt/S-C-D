using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_21_3 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 19;

        public string Description => "Some reports are now in local currency";

        public Migration_2019_02_21_3(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-20-17-11.sql");
        }
    }
}
