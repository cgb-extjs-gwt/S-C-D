using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_03_05_12_54 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 162;

        public string Description => "Fix locap reports, add SLA columns";

        public Migration_2020_03_05_12_54(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-03-05-12-54.sql");
        }
    }
}
