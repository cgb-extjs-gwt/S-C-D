using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_12_30_11_40 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 137;

        public string Description => "'IsNotified' field adding in Wg";

        public Migration_2019_12_30_11_40(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            this.repositorySet.ExecuteSql("ALTER TABLE [InputAtoms].[Wg] ADD [IsNotified] BIT NOT NULL DEFAULT (0)");
            this.repositorySet.ExecuteSql("UPDATE [InputAtoms].[Wg] SET [IsNotified] = 1");
        }
    }
}
