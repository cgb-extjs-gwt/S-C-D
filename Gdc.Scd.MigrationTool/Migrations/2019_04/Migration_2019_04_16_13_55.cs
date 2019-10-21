using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_16_13_55 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 78;

        public string Description => "Fix reinsurance calc";

        public Migration_2019_04_16_13_55(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-16-13-55.sql");
        }
    }
}
