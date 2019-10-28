using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_09_16_11_32 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 120;

        public string Description => "Add Availability Fee input level by company";

        public Migration_2019_09_16_11_32(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-09-16-11-32.sql");
        }
    }
}
