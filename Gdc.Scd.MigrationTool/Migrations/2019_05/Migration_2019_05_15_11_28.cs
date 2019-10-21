using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_15_11_28 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 102;

        public string Description => "Fix availability fee calc for empty install base";

        public Migration_2019_05_15_11_28(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-15-11-28.sql");
        }
    }
}
