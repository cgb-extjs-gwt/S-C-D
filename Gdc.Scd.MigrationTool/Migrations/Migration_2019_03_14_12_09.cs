using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_14_12_09 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 39;

        public string Description => "Logistic Reports. Remove central reports. Rename column 'Alias Region' to 'Region'";

        public Migration_2019_03_14_12_09(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-14-14-11.sql");
        }
    }
}
