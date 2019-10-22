using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_09_25_17_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 121;

        public string Description => "Fix hw overview report, add prolongation";

        public Migration_2019_09_25_17_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-09-25-17-20.sql");
        }
    }
}
