using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_28_13_56 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 24;

        public string Description => "ProActive = none by default for reports";

        public Migration_2019_02_28_13_56(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-28-13-56.sql");
        }
    }
}
