using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_03_20_14_19 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 163;

        public string Description => "Fix get calc member, skip STDW & Credits for prolongation";

        public Migration_2020_03_20_14_19(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-03-20-14-19.sql");
        }
    }
}
