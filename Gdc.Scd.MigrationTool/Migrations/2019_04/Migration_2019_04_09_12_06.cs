using System.Linq;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_09_12_06 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 76;

        public string Description => "Fix CD CS functions";

        public Migration_2019_04_09_12_06(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-09-12-06.sql");
        }
    }
}
