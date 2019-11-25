using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_28_10_07 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 107;

        public string Description => "Update trigger for installed base";

        public Migration_2019_05_28_10_07(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-28-10-07.sql");
        }
    }
}
