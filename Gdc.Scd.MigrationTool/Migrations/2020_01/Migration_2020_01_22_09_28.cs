using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_01_22_09_28 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 139;

        public string Description => "Add SODA integration functions and views";

        public Migration_2020_01_22_09_28(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-22-09-28.sql");
        }
    }
}
