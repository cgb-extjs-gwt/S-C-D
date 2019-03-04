using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_04_17_11 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 29;

        public string Description => "Rename SolutionPack-ProActive-Costing report";

        public Migration_2019_03_04_17_11(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-04-17-11.sql");
        }
    }
}
