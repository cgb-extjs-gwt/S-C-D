using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_28_17_40 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 26;

        public string Description => "Fix report locap, add calc by SOG";

        public Migration_2019_02_28_17_40(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-28-17-40.sql");
        }
    }
}
