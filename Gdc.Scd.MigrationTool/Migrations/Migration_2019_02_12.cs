using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_02_12 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 2;

        public string Description => "Add report column format, Availability FEE report change";

        public Migration_2019_02_12(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var queries = SqlFormatter.BuildFromFile(@"MigrationScripts\2019-02-12.sql");
            foreach (var query in queries)
            {
                repositorySet.ExecuteSql(query);
            }
        }
    }
}
