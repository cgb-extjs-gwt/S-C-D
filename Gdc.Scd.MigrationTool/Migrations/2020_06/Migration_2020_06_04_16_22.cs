using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_05
{
    public class Migration_2020_06_04_16_22 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public Migration_2020_06_04_16_22(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public int Number => 179;
        public string Description => "Fix hardware overview report";
        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-06-04-16-22.sql");
        }
    }
}
