using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_25 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 22;

        public string Description => "Add filter to Report.HddRetentionCalcResult";

        public Migration_2019_02_25(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-25.sql");
        }
    }
}
