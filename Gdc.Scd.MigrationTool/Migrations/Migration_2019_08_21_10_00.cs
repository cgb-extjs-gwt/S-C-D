using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_08_21_10_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 116;

        public string Description => "Fix locap reports";

        public Migration_2019_08_21_10_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-08-21-10-00.sql");
        }
    }
}
