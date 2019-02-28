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
        private readonly IRepository<Region> _repositoryRegion;
        private readonly IList<CountryGroup> _installBaseCountryGroups;
        private readonly IList<CountryGroup> _centralEuropeCountyGroups;
        private readonly IList<Country> _countries;

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
            this._repositoryRegion = this._repositorySet.GetRepository<Region>();
            this._logger = logger;
            this._installBaseCountryGroups = _repositoryCountryGroup.GetAll().Where(cg => cg.AutoUploadInstallBase).ToList();
            var centralEurope = _repositoryRegion.GetAll().FirstOrDefault(r => r.Name.Equals(InstallBaseConfig.CentralEuropeRegion, StringComparison.OrdinalIgnoreCase));
            this._centralEuropeCountyGroups = centralEurope != null ? _repositoryCountryGroup.GetAll().Where(cg => cg.RegionId == centralEurope.Id).ToList() :
                                                                     new List<CountryGroup>();
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

            //Central Europe IB calculation
            
            var batchList = new List<InstallBase>();
            var centralEuropeBatchList = new List<InstallBase>();

            var centralEuropeInstallBaseValues = new List<InstallBaseGroupingDto>();

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

                //if country belongs to central europe add it to collection and process later
                if (_centralEuropeCountyGroups.FirstOrDefault(cg => cg.Name.Equals(item.CountryGroup, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    centralEuropeInstallBaseValues.Add(item);
                    continue;
                }

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

                //adding countries that must inheirite value from another country (e.g. Luximbourg gets value from Belgium)
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

            //Upload Install Base for Central Europe
            var centralEuropeCountryGroupIds = _centralEuropeCountyGroups.Select(cg => cg.Id).ToList();
            var centralEuropeCountries = _countries.Where(c => centralEuropeCountryGroupIds.Contains(c.CountryGroupId.Value) && c.IsMaster).ToList();

            var centralEuropeValues = centralEuropeInstallBaseValues.GroupBy(ib => ib.Wg, 
                ib => ib.InstallBase, 
                (key, g) => new { Wg = key, InstallBase = g.Sum() });

            foreach (var val in centralEuropeValues)
            {
                var wg = wgs.FirstOrDefault(w => w.Name.Equals(val.Wg, StringComparison.OrdinalIgnoreCase));
                if (wg == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_WARRANTY, val.Wg);
                    continue;
                }

                foreach (var masterCountry in centralEuropeCountries)
                {
                    var installBaseDb = installBase.FirstOrDefault(ib => ib.WgId == wg.Id && ib.CountryId == masterCountry.Id && !ib.DeactivatedDateTime.HasValue);
                    if (installBaseDb == null)
                    {
                        installBaseDb = new InstallBase();
                        installBaseDb.CountryId = masterCountry.Id;
                        installBaseDb.WgId = wg.Id;
                    }
                    installBaseDb.InstalledBaseCountry = val.InstallBase;
                    installBaseDb.InstalledBaseCountry_Approved = val.InstallBase;
                    centralEuropeBatchList.Add(installBaseDb);
                }

            }

            _logger.Log(LogLevel.Info, ImportConstants.UPDATE_INSTALL_BASE_CENTRAL_EUROPE);
            if (centralEuropeBatchList.Any())
            {
                _repositoryInstallBase.Save(centralEuropeBatchList);
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_END, batchList.Count + centralEuropeBatchList.Count);

            //setting 0.0 values for IB EMEIA that wasn't received in EBIS File
            _logger.Log(LogLevel.Info, ImportConstants.SET_ZEROS_INSTALL_BASE);
            var emeiaCountryId = GetEmeiaMasterCountryIds();
            var notReceiveIbs = installBase.Where(ib => !batchList.Contains(ib) 
                                                        && !centralEuropeBatchList.Contains(ib) 
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
            allCountryGroups.AddRange(_centralEuropeCountyGroups);

            foreach (var countryGroup in allCountryGroups)
            {
                var countries = _countries.Where(c => c.IsMaster && c.CountryGroupId == countryGroup.Id).Select(c => c.Id);
                result.AddRange(countries);
            }

            return result;
        }
    }
}
