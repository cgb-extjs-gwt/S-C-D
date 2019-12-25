using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2019_12
{
    public class Migration_2019_12_25_17_03 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 136;

        public string Description => "Fix HW calc result report";

        public Migration_2019_12_25_17_03(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-12-25-17-03.sql");
        }
    }
}
