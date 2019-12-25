using Gdc.Scd.Core.Entities;
using System;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IConfigHandler
    {
        ImportConfiguration ReadConfiguration(string name);
        void UpdateImportResult(ImportConfiguration recordToUpdate, DateTime processedDateTime);
    }
}
