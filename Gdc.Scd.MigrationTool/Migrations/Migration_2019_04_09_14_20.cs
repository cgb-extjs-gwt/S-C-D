using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_09_14_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 73;

        public string Description => "Fix locap reports, add filter wg with sog";

        public Migration_2019_04_09_14_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-09-14-20.sql");
        }
    }
}
