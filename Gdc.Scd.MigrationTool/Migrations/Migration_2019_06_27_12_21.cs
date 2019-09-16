using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_06_27_12_21 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 111;

        public string Description => "Fix Tc/Tp calc, remove positive ";

        public Migration_2019_06_27_12_21(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-06-27-12-21.sql");
        }
    }
}
