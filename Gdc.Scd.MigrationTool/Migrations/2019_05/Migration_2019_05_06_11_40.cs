using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_05_06_11_40 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 95;

        public string Description => "Fix Local Global Support report, do calc by SOG for Emeia/Wg for non Emeia";

        public Migration_2019_05_06_11_40(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            repositorySet.ExecuteFromFile("2019-05-06-11-40.sql");
        }
    }
}
