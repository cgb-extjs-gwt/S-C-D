using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_08_19_00 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 72;

        public string Description => "Fix [Hardware].[SpReleaseCosts] SP";

        public Migration_2019_04_08_19_00(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-08-19-00.sql");
        }
    }
}
