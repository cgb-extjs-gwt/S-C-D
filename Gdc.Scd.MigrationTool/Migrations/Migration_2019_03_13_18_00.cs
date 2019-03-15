using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_13_18_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 40;

        public string Description => "Optimize calculation";

        public Migration_2019_03_13_18_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-13-18-00.sql");
        }
    }
}
