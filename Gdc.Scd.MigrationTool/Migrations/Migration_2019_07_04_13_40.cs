using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_07_04_13_40 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 113;

        public string Description => "Adding PRS 'PsmRelease' in WG";

        public Migration_2019_07_04_13_40(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteSql(
                "ALTER TABLE [InputAtoms].[Wg] ADD [PsmRelease] BIT NOT NULL DEFAULT(0)");

            this.repositorySet.ExecuteFromFile("2019-07-04-13-40.sql");
        }
    }
}
