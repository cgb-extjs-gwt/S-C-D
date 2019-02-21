using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_21_16_59 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 17;

        public string Description => "Fix calc standard warranty";

        public Migration_2019_02_21_16_59(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-21-15-16.sql");
        }
    }
}
