using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
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
    public class LogisticUploader : IUploader<LogisticsDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<Country> _repositoryCountry;
        private readonly IRepository<Pla> _repositoryPla;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly IRepository<AvailabilityFee> _availabilityFeeRepo;
        private readonly ILogger<LogLevel> _logger;

        public LogisticUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryPla = this._repositorySet.GetRepository<Pla>();
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
            this._repositoryCountry = this._repositorySet.GetRepository<Country>();
            this._availabilityFeeRepo = this._repositorySet.GetRepository<AvailabilityFee>();
            this._logger = logger;
        }


        public int Deactivate(DateTime modifiedDateTime)
        {
            var deactivatedWgs = _repositoryWg.GetAll().Where(wg => wg.IsDeactivatedInLogistic && wg.DeactivatedDateTime.HasValue);
            var deactivatedAvailabilityFees = new List<AvailabilityFee>();
            //TODO: Run Mechanism that deactivate other cost elements
            foreach (var wg in deactivatedWgs)
            {
                var availabilityFee = _availabilityFeeRepo.GetAll().Where(af => af.WgId == wg.Id && !af.DeactivatedDateTime.HasValue);
                foreach (var af in availabilityFee)
                {
                    af.DeactivatedDateTime = modifiedDateTime;
                    af.ModifiedDateTime = modifiedDateTime;
                    _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATING_ENTITY, $"Availability Fee with WG {wg.Name}", af.CountryId);
                    deactivatedAvailabilityFees.Add(af);
                }
            }

            if (deactivatedAvailabilityFees.Any())
            {
                _availabilityFeeRepo.Save(deactivatedAvailabilityFees);
                _repositorySet.Sync();
            }

            return deactivatedAvailabilityFees.Count;
        }

        public void Upload(IEnumerable<LogisticsDto> items, DateTime modifiedDateTime)
        {
            UpdateWg(items, modifiedDateTime);
            //TODO: INCLUDE MECHANISM FOR UPDATING COST BLOCKS AFTER NEW WG WAS ADDED 
            var result = UpdateLogistic(items, modifiedDateTime);
            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_AVAILABILITY_FEE_END, result);    
        }

        private void UpdateWg(IEnumerable<LogisticsDto> items, DateTime modifiedDateTime)
        {
            var plas = _repositoryPla.GetAll().ToList();
            var wgs = _repositoryWg.GetAll().ToList();

            var batchUpdate = new List<Wg>();

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_WG_START);
            foreach (var item in items)
            {
                var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.WgCode));
                switch (item.Action.ToLower())
                {
                    case "n":
                    case "u":
                        var pla = plas.FirstOrDefault(p => !String.IsNullOrEmpty(p.CodingPattern) && p.CodingPattern.Equals(item.Pla, StringComparison.OrdinalIgnoreCase));
                        if (pla == null)
                        {
                            _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_PLA, nameof(Wg), item.WgCode, item.Pla);
                            continue;
                        }
                        //If Wg does not exist in the SCD database then create new
                        if (wg == null)
                        {
                            item.PlaId = pla.Id;

                            _logger.Log(LogLevel.Info, ImportConstants.NEW_WG, item.WgCode);
                            batchUpdate.Add(new Wg
                            {
                                CreatedDateTime = DateTime.Now,
                                Description = item.WgDescription,
                                PlaId = item.PlaId.Value,
                                Name = item.WgCode,
                                ExistsInLogisticsDb = true,
                                IsMultiVendor = item.IsMultiVendor,
                                ModifiedDateTime = DateTime.Now,
                                IsSoftware = false,
                                IsDeactivatedInLogistic = false
                            });
                        }

                        //if WG is already exist in SCD Database
                        else
                        {
                            wg.IsDeactivatedInLogistic = false;
                            wg.DeactivatedDateTime = null;
                            wg.ModifiedDateTime = modifiedDateTime;
                            wg.ExistsInLogisticsDb = true;
                            wg.IsMultiVendor = item.IsMultiVendor;
                            //IF WG is Multi Vendor update description and PLA
                            if (item.IsMultiVendor)
                            {
                                wg.PlaId = pla.Id;
                                wg.Description = item.WgDescription;
                            }
                            _logger.Log(LogLevel.Info, ImportConstants.UPDATE_WG, item.WgCode);
                            batchUpdate.Add(wg);

                        }
                        break;
                    case "d":
                        //deactivating WG
                        if (wg != null)
                        {
                            wg.ExistsInLogisticsDb = true;
                            wg.IsDeactivatedInLogistic = true;
                            wg.ModifiedDateTime = modifiedDateTime;
                            if (wg.IsMultiVendor)
                                wg.DeactivatedDateTime = modifiedDateTime;
                            _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATE_START, item.WgCode);
                            batchUpdate.Add(wg);
                        }
                        break;
                }
            }

            if (batchUpdate.Any())
            {
                _repositoryWg.Save(batchUpdate);
                _repositorySet.Sync();
            }
            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_WG_END, batchUpdate.Count);
        }

        private int UpdateLogistic(IEnumerable<LogisticsDto> items, DateTime modifiedDateTime)
        {
            var countries = _repositoryCountry.GetAll().Where(c => c.IsMaster).Select(c => c.Id).ToList();
            var multiVendorCountries = _repositoryCountry.GetAll().Where(c => c.IsMaster && c.AssignedToMultiVendor).Select(c => c.Id).ToList();
            var wgs = _repositoryWg.GetAll().Where(wg => !wg.DeactivatedDateTime.HasValue).ToList();
            var result = 0;

            foreach (var item in items)
            {
                switch (item.Action.ToLower())
                {
                    case "n":
                    case "u":
                        var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.WgCode, StringComparison.OrdinalIgnoreCase));
                        if (wg != null)
                        {
                            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_AVAILABILITY_FEE_START, wg.Name);
                            Func<AvailabilityFee, bool> pred = null;
                            if (item.IsMultiVendor)
                                pred = af => af.WgId == wg.Id && multiVendorCountries.Contains(af.CountryId.Value);
                            else
                                pred = af => af.WgId == wg.Id && countries.Contains(af.CountryId.Value);

                            var itemsToUpdate = _availabilityFeeRepo.GetAll().Where(pred).ToList();
                            var batchList = new List<AvailabilityFee>();
                            foreach (var itemToUpdate in itemsToUpdate)
                            {
                                if (item.IsJapanCostPerKit.ToLower() == "y")
                                    itemToUpdate.CostPerKitJapanBuy = item.CostPerKit;
                                else
                                    itemToUpdate.CostPerKit = item.CostPerKit;
                                itemToUpdate.MaxQty = item.MaxQty;
                                itemToUpdate.DeactivatedDateTime = null;
                                itemToUpdate.ModifiedDateTime = modifiedDateTime;
                                batchList.Add(itemToUpdate);
                            }

                            if (batchList.Any())
                            {
                                _availabilityFeeRepo.Save(batchList);
                                _repositorySet.Sync();
                                result += batchList.Count;
                            }
                        }
                        break;
                    case "d":
                        continue;
                }
            }

            return result;
        }
    }
}
