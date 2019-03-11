using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_11_15_30 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 34;

        public string Description => "Add permissions for 'GTS User' role";


        public Migration_2019_03_11_15_30(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-07-13-12.sql");
        }
    }
}
