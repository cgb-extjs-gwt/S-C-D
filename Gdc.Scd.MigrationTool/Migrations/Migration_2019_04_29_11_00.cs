using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_29_11_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 92;

        public string Description => "Fix calculation Release Service TP with reinsurance, get reinsurance for prolongation";

        public Migration_2019_04_29_11_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-29-11-00.sql");
        }
    }
}
