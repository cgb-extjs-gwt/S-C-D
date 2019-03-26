using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_26_13_50 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 55;

        public string Description => "Fix logistic view, report duplicates";

        public Migration_2019_03_26_13_50(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-26-13-50.sql");
        }
    }
}
