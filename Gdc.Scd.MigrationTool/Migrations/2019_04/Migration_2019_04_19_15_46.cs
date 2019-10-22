using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_19_15_46 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 84;

        public string Description => "Add released date to locap reports";

        public Migration_2019_04_19_15_46(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-19-15-46.sql");
        }
    }
}
