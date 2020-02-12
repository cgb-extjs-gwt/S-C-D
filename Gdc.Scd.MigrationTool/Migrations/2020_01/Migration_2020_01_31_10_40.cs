using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_01_31_10_40 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 146;

        public string Description => "Fix HW calc, add reactive TC/TP";

        public Migration_2020_01_31_10_40(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2020-01-31-10-40.sql");
        }
    }
}
