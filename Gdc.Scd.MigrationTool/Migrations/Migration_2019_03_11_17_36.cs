using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_11_17_36 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 36;

        public string Description => "Add standard warranty service location to client result table";


        public Migration_2019_03_11_17_36(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-11-17-36.sql");
        }
    }
}
