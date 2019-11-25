using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_30_11_04 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 93;

        public string Description => "Fix standard warranty calculation, add sum with Availability Fee";

        public Migration_2019_04_30_11_04(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-30-11-04.sql");
        }
    }
}
