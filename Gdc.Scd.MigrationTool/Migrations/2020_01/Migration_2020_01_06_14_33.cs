using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_01_06_14_33 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 138;

        public string Description => "Fix SW list and Solution pack list reports";

        public Migration_2020_01_06_14_33(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-06-14-33.sql");
        }
    }
}
