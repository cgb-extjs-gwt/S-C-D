using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_22_14_47 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 88;

        public string Description => "Fix sw overview report, sw support calculation";

        public Migration_2019_04_22_14_47(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-22-14-47.sql");
        }
    }
}
