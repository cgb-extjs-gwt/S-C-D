using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_101 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 6;

        public string Description => "Add report column format, Availability FEE report change";

        public Migration_2019_02_101(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var queries = SqlFormatter.BuildFromFile(@"MigrationScripts\2019-02-101.sql");
            foreach (var query in queries)
            {
                repositorySet.ExecuteSql(query);
            }
        }
    }
}
