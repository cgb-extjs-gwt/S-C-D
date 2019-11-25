using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_22_16_57 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 89;

        public string Description => "Fix sw price reports";

        public Migration_2019_04_22_16_57(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-22-16-57.sql");
        }
    }
}
