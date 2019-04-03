using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_03_16_04 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 67;

        public string Description => "Add afr, standard warranty reports";

        public Migration_2019_04_03_16_04(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-03-16-04.sql");
        }
    }
}
