using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_01_23_15_19 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 141;

        public string Description => "Fix proactive calculation, set zero for proactive(none)";

        public Migration_2020_01_23_15_19(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-23-15-19.sql");
        }
    }
}
