using System.Collections.Generic;

namespace Gdc.Scd.MigrationTool.Interfaces
{
    public interface IMigrationService
    {
        IEnumerable<IMigrationAction> GetMigrationActions();

        void SaveMigrationAsExecuted(IMigrationAction migrationAction);
    }
}
