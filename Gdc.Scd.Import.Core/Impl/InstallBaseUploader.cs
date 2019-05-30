using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IList<CountryGroup> _installBaseCountryGroups;
        private readonly IList<Country> _countries;

        public InstallBaseUploader(IRepositorySet repositorySet, ImportRepository<InstallBase> ibRepo, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
            this._repositoryCountry = this._repositorySet.GetRepository<Country>();
            this._repositoryInstallBase = ibRepo;
            this._repositoryCountryGroup = this._repositorySet.GetRepository<CountryGroup>();
            this._logger = logger;
            this._installBaseCountryGroups = _repositoryCountryGroup.GetAll().Where(cg => cg.AutoUploadInstallBase).ToList();
            this._countries = _repositoryCountry.GetAll().ToList();
        }

        public IEnumerable<UpdateQueryOption> Upload(IEnumerable<InstallBaseDto> items, DateTime modifiedDateTime)
        {
            var wgs = _repositoryWg.GetAll().Where(wg => wg.WgType == WgType.Por && !wg.IsSoftware).ToList();
            var installBase = _repositoryInstallBase.GetAll().Where(ib => !ib.DeactivatedDateTime.HasValue).ToList();


            var countryMatches = new Dictionary<long, long>();
            foreach (var cm in InstallBaseConfig.CountryMatch)
            {
                var countrySource = _countries.FirstOrDefault(c => c.Name.Equals(cm.Key, StringComparison.OrdinalIgnoreCase) && c.IsMaster);
                var countryTarget = _countries.FirstOrDefault(c => c.Name.Equals(cm.Value, StringComparison.OrdinalIgnoreCase) && c.IsMaster);
                if (countrySource != null && countryTarget != null)
                    countryMatches.Add(countrySource.Id, countryTarget.Id);
            }

            var batchList = new List<InstallBase>();

            var ibGroupedByCountryGroupAndWg = from ib in items
                                               group ib by new
                                               {
                                                   ib.CountryGroup,
                                                   ib.Wg
                                               } into ibg
                                               select new InstallBaseGroupingDto
                                               {
                                                   CountryGroup = ibg.Key.CountryGroup,
                                                   Wg = ibg.Key.Wg,
                                                   InstallBase = ibg.Sum(i => i.InstallBase)
                                               };

            foreach (var item in ibGroupedByCountryGroupAndWg)
            {
                if (String.IsNullOrEmpty(item.Wg) || item.Wg.Equals("-") || String.IsNullOrEmpty(item.CountryGroup))
                    continue;

                var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.Wg, StringComparison.OrdinalIgnoreCase));
                if (wg == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_WARRANTY, item.Wg);
                    continue;
                }

                var countryGroup = _installBaseCountryGroups.FirstOrDefault(c => c.Name.Equals(item.CountryGroup, StringComparison.OrdinalIgnoreCase));
                if (countryGroup == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_COUNTRY_CODE, item.CountryGroup);
                    continue;
                }

                //getting master countries in country group
                var masterCountryIds = _countries.Where(c => c.CountryGroupId == countryGroup.Id && c.IsMaster).Select(c => c.Id).ToList();

                //adding countries that must inherit value from another country (e.g. Luximbourg gets value from Belgium)
                foreach (var countryMatch in countryMatches)
                {
                    if (masterCountryIds.Contains(countryMatch.Value))
                        masterCountryIds.Add(countryMatch.Key);
                }

                foreach (var masterCountryId in masterCountryIds)
                {
                    var installBaseDb = installBase.FirstOrDefault(ib => ib.WgId == wg.Id && ib.CountryId == masterCountryId);

                    if (installBaseDb == null)
                    {
                        installBaseDb = new InstallBase();
                        installBaseDb.CountryId = masterCountryId;
                        installBaseDb.WgId = wg.Id;
                    }

                    installBaseDb.InstalledBaseCountry = item.InstallBase;
                    installBaseDb.InstalledBaseCountry_Approved = item.InstallBase;
                    batchList.Add(installBaseDb);
                }
            }

            _repositoryInstallBase.DisableTrigger();

            if (batchList.Any())
            {
                _repositoryInstallBase.Save(batchList);
            }

            //setting 0.0 values for IB EMEIA that wasn't received in EBIS File
            _logger.Log(LogLevel.Info, ImportConstants.SET_ZEROS_INSTALL_BASE);
            var emeiaCountryId = GetEmeiaMasterCountryIds();
            var notReceiveIbs = installBase.Where(ib => !batchList.Contains(ib)
                                                        && emeiaCountryId.Contains(ib.CountryId.Value)).ToList();
            foreach (var ib in notReceiveIbs)
            {
                ib.InstalledBaseCountry = 0.0;
                ib.InstalledBaseCountry_Approved = 0.0;
            }
            if (notReceiveIbs.Any())
            {
                _repositoryInstallBase.Save(notReceiveIbs);
            }

            _repositoryInstallBase.EnableTrigger();

            _logger.Log(LogLevel.Info, ImportConstants.ZEROS_SET, notReceiveIbs.Count);
            return new List<UpdateQueryOption>();
        }

        private List<long> GetEmeiaMasterCountryIds()
        {
            var result = new List<long>();
            var allCountryGroups = new List<CountryGroup>(_installBaseCountryGroups);

            foreach (var countryGroup in allCountryGroups)
            {
                var countries = _countries.Where(c => c.IsMaster && c.CountryGroupId == countryGroup.Id).Select(c => c.Id);
                result.AddRange(countries);
            }

            return result;
        }
    }
}
