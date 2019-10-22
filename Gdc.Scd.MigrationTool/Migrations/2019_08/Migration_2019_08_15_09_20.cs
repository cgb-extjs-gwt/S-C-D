using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_08_15_09_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 115;

        public string Description => "Fix ProActive report";

        public Migration_2019_08_15_09_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-08-15-09-20.sql");
        }
    }
}
