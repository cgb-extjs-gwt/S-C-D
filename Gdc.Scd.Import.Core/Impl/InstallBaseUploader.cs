using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class InstallBaseUploader : IUploader<InstallBaseDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<Country> _repositoryCountry;
        private readonly IRepository<CountryGroup> _repositoryCountryGroup;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly IRepository<InstallBase> _repositoryInstallBase;
        private readonly ILogger<LogLevel> _logger;

        public InstallBaseUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
            this._repositoryCountry = this._repositorySet.GetRepository<Country>();
            this._repositoryInstallBase = this._repositorySet.GetRepository<InstallBase>();
            this._repositoryCountryGroup = this._repositorySet.GetRepository<CountryGroup>();
            this._logger = logger;
        }

        public IEnumerable<UpdateQueryOption> Upload(IEnumerable<InstallBaseDto> items, DateTime modifiedDateTime)
        {
            var wgs = _repositoryWg.GetAll().Where(wg => wg.WgType == WgType.Por && !wg.IsSoftware).ToList();
            var countryGroups = _repositoryCountryGroup.GetAll().Where(cg => cg.AutoUploadInstallBase).ToList();
            var countries = _repositoryCountry.GetAll().ToList();
            var installBase = _repositoryInstallBase.GetAll().ToList();

            var batchList = new List<InstallBase>();

            foreach (var item in items)
            {
                if (String.IsNullOrEmpty(item.Wg) || item.Wg.Equals("-") || String.IsNullOrEmpty(item.CountryGroup))
                    continue;
                var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.Wg, StringComparison.OrdinalIgnoreCase));
                if (wg == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_WARRANTY, item.Wg);
                    continue;
                }
                var countryGroup = countryGroups.FirstOrDefault(c => c.Name.Equals(item.CountryGroup, StringComparison.OrdinalIgnoreCase));
                if (countryGroup == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_COUNTRY_CODE, item.CountryGroup);
                    continue;
                }
                //getting master countries in country group
                var masterCountries = countries.Where(c => c.CountryGroupId == countryGroup.Id && c.IsMaster);

                foreach (var masterCountry in masterCountries)
                {
                    var installBaseDb = installBase.FirstOrDefault(ib => ib.WgId == wg.Id && ib.CountryId == masterCountry.Id);
                    if (installBaseDb == null)
                    {
                        installBaseDb = new InstallBase();
                        installBaseDb.CountryId = masterCountry.Id;
                        installBaseDb.WgId = wg.Id;
                        installBaseDb.CentralContractGroupId = wg.CentralContractGroupId;
                        installBaseDb.PlaId = wg.PlaId;
                    }
                    installBaseDb.InstalledBaseCountry = item.InstallBase;
                    batchList.Add(installBaseDb);
                }
            }

            if (batchList.Any())
            {
                  _repositoryInstallBase.Save(batchList);
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_END, batchList.Count);
            return new List<UpdateQueryOption>();
        }
    }
}
