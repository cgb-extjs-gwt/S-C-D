using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_05_09_12 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 69;

        public string Description => "Change HW calculation, revert negative values";

        public Migration_2019_04_05_09_12(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-05-09-12.sql");
        }
    }
}
