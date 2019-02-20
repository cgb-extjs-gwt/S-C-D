using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_18 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 13;

        public string Description => "Fix software calculation Service Support Cost";

        public Migration_2019_02_18(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-18.sql");
        }
    }
}
