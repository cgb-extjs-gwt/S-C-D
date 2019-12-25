using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2019_12
{
    public class Migration_2019_12_25_09_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 135;

        public string Description => "Fix STDW overview report, add FSP code";

        public Migration_2019_12_25_09_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-25-09-20.sql");
        }
    }
}
