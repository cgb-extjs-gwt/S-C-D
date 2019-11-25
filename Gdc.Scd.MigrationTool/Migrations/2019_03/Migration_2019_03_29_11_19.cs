using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_29_11_19 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 62;

        public string Description => "Fix software solution pack reports";

        public Migration_2019_03_29_11_19(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-29-11-19.sql");
        }
    }
}
