using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_07_14_16_02 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public string Description => "Add plausibility check functions";

        public int Number => 187;

        public Migration_2020_07_14_16_02(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-07-14-16-02.sql");
        }
    }
}
