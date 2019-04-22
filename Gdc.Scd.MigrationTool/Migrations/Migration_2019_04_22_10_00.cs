using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_22_10_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 87;

        public string Description => "Fix getcost by sla sog function. Change contract report, fix monthly service tp values";

        public Migration_2019_04_22_10_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-22-10-00.sql");
        }
    }
}
