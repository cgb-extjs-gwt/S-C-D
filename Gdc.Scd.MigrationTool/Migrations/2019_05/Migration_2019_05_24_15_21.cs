using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_24_15_21 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 105;

        public string Description => "Add input Standard warranty manual cost";

        public Migration_2019_05_24_15_21(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-24-15-21.sql");
        }
    }
}
