using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_07_29_17_03 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public string Description => "Fix spRelease cost, change release manuals";

        public int Number => 190;

        public Migration_2020_07_29_17_03(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-07-29-17-03.sql");
        }
    }
}
