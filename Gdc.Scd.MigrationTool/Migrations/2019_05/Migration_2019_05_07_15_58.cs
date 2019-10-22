using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_07_15_58 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 99;

        public string Description => "Fix release cost calculation, remove auto sum for ServiceTP_Released";

        public Migration_2019_05_07_15_58(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-07-15-58.sql");
        }
    }
}
