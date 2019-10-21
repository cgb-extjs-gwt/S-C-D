using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_05_11_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 31;

        public string Description => "Normalize field service cost table";

        public Migration_2019_03_05_11_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-05-11-00.sql");
        }
    }
}
