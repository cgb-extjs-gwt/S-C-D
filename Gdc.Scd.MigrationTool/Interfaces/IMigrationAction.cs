using System;

namespace Gdc.Scd.MigrationTool.Interfaces
{
    public interface IMigrationAction
    {
        int Number { get; }

        string Description { get; }

        void Execute();
    }
}
