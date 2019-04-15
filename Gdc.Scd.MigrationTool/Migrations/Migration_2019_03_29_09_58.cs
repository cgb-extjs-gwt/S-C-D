using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_29_09_58 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 61;

        public string Description => "Fix standard warranty, out of warranty calculation";

        public Migration_2019_03_29_09_58(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-29-09-58.sql");
        }
    }
}
