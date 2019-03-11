using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_11_12_39 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 33;

        public string Description => "Add software parameter overview report";

        public Migration_2019_03_11_12_39(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-11-12-39.sql");
        }
    }
}
