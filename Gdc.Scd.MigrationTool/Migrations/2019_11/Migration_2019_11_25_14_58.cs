using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_11_25_14_58 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 128;

        public string Description => "Change Locap global support realised report";

        public Migration_2019_11_25_14_58(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-11-25-14-58.sql");
        }
    }
}
