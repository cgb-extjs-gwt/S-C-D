using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IConfigHandler
    {
        ImportConfiguration ReadConfiguration(string name);
        void UpdateImportResult(ImportConfiguration recordToUpdate, DateTime processedDateTime);
    }
}
