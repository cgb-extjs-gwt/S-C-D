using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_18_17_43 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 81;

        public string Description => "CD CS use released yearly costs";

        public Migration_2019_04_18_17_43(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-18-17-43.sql");
        }
    }
}
