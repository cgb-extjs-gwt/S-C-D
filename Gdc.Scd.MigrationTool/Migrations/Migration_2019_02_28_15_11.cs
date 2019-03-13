using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_28_15_11 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 25;

        public string Description => "Fix report-calc-parameter-hw report";

        public Migration_2019_02_28_15_11(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-28-15-11.sql");
        }
    }
}
