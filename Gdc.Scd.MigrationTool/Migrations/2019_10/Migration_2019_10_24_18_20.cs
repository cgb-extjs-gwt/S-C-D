using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_10_24_18_20 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 127;

        public string Description => "Add fsp to calc result";

        public Migration_2019_10_24_18_20(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-10-24-18-20.sql");
        }
    }
}
