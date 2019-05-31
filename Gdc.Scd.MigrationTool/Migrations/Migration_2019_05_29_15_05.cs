using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_29_15_05 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 108;

        public string Description => "Fix reports to use manual Stdw";

        public Migration_2019_05_29_15_05(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-29-15-05.sql");
        }
    }
}
