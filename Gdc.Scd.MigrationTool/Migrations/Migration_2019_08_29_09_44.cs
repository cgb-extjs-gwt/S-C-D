using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_08_29_09_44 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 117;

        public string Description => "Fix proactive report, add calc by sog";

        public Migration_2019_08_29_09_44(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-08-29-09-44.sql");
        }
    }
}
