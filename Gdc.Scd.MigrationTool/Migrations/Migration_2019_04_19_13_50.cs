using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_04_19_13_50 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 84;

        public string Description => "Change FSP standard warranty insert, add compare by LUT";

        public Migration_2019_04_19_13_50(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-04-19-13-50.sql");
        }
    }
}
