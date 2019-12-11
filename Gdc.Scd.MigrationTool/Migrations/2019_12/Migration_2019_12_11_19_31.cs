using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_12_11_19_31 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 131;

        public string Description => "Fix contract report";

        public Migration_2019_12_11_19_31(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-11-19-31.sql");
        }
    }
}
