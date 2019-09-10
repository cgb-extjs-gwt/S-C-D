using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_09_06_15_55 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 118;

        public string Description => "Merge prolongation markup and markup other costs";

        public Migration_2019_09_06_15_55(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-09-06-15-55.sql");
        }
    }
}
