using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_19_10_44 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 83;

        public string Description => "Fix getcost by sla sog function, fix contract report";

        public Migration_2019_04_19_10_44(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-19-10-44.sql");
        }
    }
}
