using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_01
{
    public class Migration_2020_01_23_20_08 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 141;

        public string Description => "Fix InputAtoms.WgSogView view creation";

        public Migration_2020_01_23_20_08(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-23-20-08.sql");
        }
    }
}