using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_01
{
    public class Migration_2020_01_24_13_34 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 143;

        public string Description => "Fix locap reports";

        public Migration_2020_01_24_13_34(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-24-13-34.sql");
        }
    }
}