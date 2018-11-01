using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs.Impl
{
    public class ConfigHandler
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<CdCsConfiguration> _repository;

        public ConfigHandler(IRepositorySet repositorySet)
        {
            this._repositorySet = repositorySet ?? throw new ArgumentNullException(nameof(repositorySet));
            this._repository = this._repositorySet.GetRepository<CdCsConfiguration>();
        }

        public List<CdCsConfiguration> ReadAllConfiguration()
        {
            var configuration = this._repository.GetAll().Include(x => x.Country).Include(x => x.Country.Currency);
            if (configuration == null)
                throw new ConfigurationErrorsException($"Configuration error: Config doesn't exist in the database");
            return configuration.ToList();
        }
    }
}
