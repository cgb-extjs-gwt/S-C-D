using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_21 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 15;

        public string Description => "Removing ReactionTimeTypeAvailability dependency from MarkupStandardWaranty";

        public Migration_2019_02_21(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-02-21.sql");
        }
    }
}
