using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.Configuration;
using System.Linq;

namespace Gdc.Scd.Import.Core.Impl
{
    public class DataBaseConfigHandler : IConfigHandler
    {
        protected readonly IRepositorySet _repositorySet;
        protected readonly IRepository<ImportConfiguration> _repository;

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

        public virtual void UpdateImportResult(ImportConfiguration recordToUpdate, 
            DateTime processedDateTime)
        {
            recordToUpdate.ProcessedDateTime = processedDateTime;
            this._repository.Save(recordToUpdate);
            this._repositorySet.Sync();
        }
    }
}
