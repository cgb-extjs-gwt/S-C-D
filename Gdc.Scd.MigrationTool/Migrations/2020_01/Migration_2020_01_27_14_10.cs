using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_01
{
    public class Migration_2020_01_27_14_10 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 144;

        public string Description => "Fix availability fee report, change column heading";

        public Migration_2020_01_27_14_10(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-27-14-10.sql");
        }
    }
}