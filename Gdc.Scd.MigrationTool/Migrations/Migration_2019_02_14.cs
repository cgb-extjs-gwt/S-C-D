using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_14 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 9;

        public string Description => "Fix Tax&duties out of warranty";

        public Migration_2019_02_14(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-14.sql");
            repositorySet.ExecuteFromFile("2019-02-14-2.sql");
        }
    }
}
