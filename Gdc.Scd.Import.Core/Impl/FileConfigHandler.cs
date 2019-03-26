using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Gdc.Scd.Core.Enums;

namespace Gdc.Scd.Import.Core.Impl
{
    public class FileConfigHandler : IConfigHandler
    {
        public ImportConfiguration ReadConfiguration(string name)
        {
            var configuration = new ImportConfiguration
            {
                Culture = ConfigurationManager.AppSettings["Culture"],
                Delimeter = ConfigurationManager.AppSettings["Delimeter"],
                FileName = ConfigurationManager.AppSettings["FileName"],
                FilePath = ConfigurationManager.AppSettings["FilePath"],
                HasHeader = Boolean.Parse(ConfigurationManager.AppSettings["HasHeader"]),
                ProcessedFilesPath = ConfigurationManager.AppSettings["ProcessedFilesPath"]
            };

            return configuration;
        }

        public void UpdateImportResult(ImportConfiguration recordToUpdate, 
            DateTime processedDateTime)
        {
            
        }
    }
}
