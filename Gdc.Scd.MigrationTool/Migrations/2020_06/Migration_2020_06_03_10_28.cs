using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_05
{
    public class Migration_2020_06_03_10_28 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public Migration_2020_06_03_10_28(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public int Number => 178;
        public string Description => "Add STDW detailed report. Add it to locap report as part";
        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-06-03-10-28.sql");
        }
    }
}
