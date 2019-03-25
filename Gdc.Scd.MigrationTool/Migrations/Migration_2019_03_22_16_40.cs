using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_22_16_40 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 53;

        public string Description => "Optimize hardware calculation";

        public Migration_2019_03_22_16_40(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-22-16-40.sql");
        }
    }
}
