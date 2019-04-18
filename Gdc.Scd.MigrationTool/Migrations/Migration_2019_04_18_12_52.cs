using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_18_12_52 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 80;

        public string Description => "Fix getcost by sla sog function, fix contract report";

        public Migration_2019_04_18_12_52(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-18-12-52.sql");
        }
    }
}
