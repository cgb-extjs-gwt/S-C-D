using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_27_12_28 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 55;

        public string Description => "Availability fee report cost per KIT should be always shown in EUR";

        public Migration_2019_03_27_12_28(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-03-27-12-28.sql");
        }
    }
}
