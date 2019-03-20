using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_20_14_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 47;

        public string Description => "Change FSP sla hash";

        public Migration_2019_03_20_14_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-20-14-00.sql");
        }
    }
}
