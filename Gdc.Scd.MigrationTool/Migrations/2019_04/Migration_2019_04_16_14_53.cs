using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_16_14_53 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 79;

        public string Description => "Add change date to manual cost";

        public Migration_2019_04_16_14_53(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-16-14-53.sql");
        }
    }
}
