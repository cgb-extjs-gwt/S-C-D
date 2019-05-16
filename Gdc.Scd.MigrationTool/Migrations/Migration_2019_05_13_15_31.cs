using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_13_15_31 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 100;

        public string Description => "Fix [Report].[SwParamOverview]";

        public Migration_2019_05_13_15_31(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-13-15-31.sql");
        }
    }
}
