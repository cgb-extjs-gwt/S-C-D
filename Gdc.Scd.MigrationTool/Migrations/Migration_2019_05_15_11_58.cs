using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_15_11_58 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 103;

        public string Description => "Add CanOverride2ndLevelSupportLocal to country table";

        public Migration_2019_05_15_11_58(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-14-12-26.sql");
        }
    }
}
