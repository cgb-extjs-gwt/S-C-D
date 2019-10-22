using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 15;

        public string Description => "Add cost editor permissions to PRS PSM";

        public Migration_2019_02_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-20.sql");
        }
    }
}
