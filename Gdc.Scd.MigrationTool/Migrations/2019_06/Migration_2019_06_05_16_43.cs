using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_06_05_16_43 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 109;

        public string Description => "Fix reports Hw overview, fix calc reinsurance";

        public Migration_2019_06_05_16_43(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-06-05-16-43.sql");
        }
    }
}
