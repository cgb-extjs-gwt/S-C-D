using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_12_11_20_10 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 132;

        public string Description => "Fix proactive costing overview report";

        public Migration_2019_12_11_20_10(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-11-20-10.sql");
        }
    }
}
