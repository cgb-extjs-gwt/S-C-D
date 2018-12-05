using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class DataBaseConfigHandler : IConfigHandler
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<ImportConfiguration> _repository;

        public DataBaseConfigHandler(IRepositorySet repositorySet)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));
            this._repositorySet = repositorySet;
            this._repository = this._repositorySet.GetRepository<ImportConfiguration>();
        }

        public ImportConfiguration ReadConfiguration(string name)
        {
            var configuration = this._repository.GetAll().FirstOrDefault(config => config.Name == name);
            if (configuration == null)
                throw new ConfigurationErrorsException($"Configuration error: Config for {name} doesn't exist in the database");
            return configuration;
        }

        public void UpdateImportResult(ImportConfiguration recordToUpdate, 
            DateTime processedDateTime)
        {
            recordToUpdate.ProcessedDateTime = processedDateTime;
            this._repository.Save(recordToUpdate);
            this._repositorySet.Sync();
        }
    }
}
