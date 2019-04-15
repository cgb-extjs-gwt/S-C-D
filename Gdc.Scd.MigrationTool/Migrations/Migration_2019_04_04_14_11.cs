using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_04_14_11 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 68;

        public string Description => "Add afr, standard warranty reports";

        public Migration_2019_04_04_14_11(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-04-14-11.sql");
        }
    }
}
