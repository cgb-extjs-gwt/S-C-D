using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_11_28_12_45 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 129;

        public string Description => "Add new Service Location Item 'On-Site Exchange'";

        public Migration_2019_11_28_12_45(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteFromFile("2019-11-28-12-45.sql");
        }
    }
}
