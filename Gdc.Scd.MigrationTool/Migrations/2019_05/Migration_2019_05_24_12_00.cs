using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_24_12_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 104;

        public string Description => "Add duration filter to Hw overview reports";

        public Migration_2019_05_24_12_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-24-12-00.sql");
        }
    }
}
