using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_21_17_30 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 20;

        public string Description => "Fix calc standard warranty";

        public Migration_2019_02_21_17_30(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-21-17-30.sql");
        }
    }
}
