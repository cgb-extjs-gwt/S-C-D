using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Core.Impl
{
    public class LogisticUploader : IUploader<LogisticsDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<Country> _repositoryCountry;
        private readonly IRepository<Pla> _repositoryPla;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly IRepository<AvailabilityFeeWg> _availabilityFeeRepo;
        private readonly IRepository<CentralContractGroup> _centralContractGroupRepo;
        private readonly ICostBlockService costBlockService;
        private readonly ILogger<LogLevel> _logger;
        private List<Wg> _newlyAddedWgs = new List<Wg>();
        private List<long> _deletedWgs = new List<long>();
        private List<long> _allCountries;
        private List<long> _multiVendorCountries;

        public LogisticUploader(
            IRepositorySet repositorySet,
            ICostBlockService costBlockService,
            ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.costBlockService = costBlockService;
            this._repositorySet = repositorySet;
            this._repositoryPla = this._repositorySet.GetRepository<Pla>();
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
            this._repositoryCountry = this._repositorySet.GetRepository<Country>();
            this._availabilityFeeRepo = this._repositorySet.GetRepository<AvailabilityFeeWg>();
            this._centralContractGroupRepo = this._repositorySet.GetRepository<CentralContractGroup>();
            this._logger = logger;
            this._allCountries = _repositoryCountry.GetAll().Where(c => c.IsMaster).Select(c => c.Id).ToList();
            this._multiVendorCountries = _repositoryCountry.GetAll().Where(c => c.IsMaster && c.AssignedToMultiVendor).Select(c => c.Id).ToList();
        }

        public IEnumerable<UpdateQueryOption> Upload(IEnumerable<LogisticsDto> items, DateTime modifiedDateTime)
        {
            UpdateWg(items, modifiedDateTime);
            this.costBlockService.UpdateByCoordinates(MetaConstants.WgInputLevelName);
            
            var result = UpdateLogistic(items, modifiedDateTime);
             _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_AVAILABILITY_FEE_END, result);

            return new List<UpdateQueryOption>();
        }

        private void UpdateWg(IEnumerable<LogisticsDto> items, DateTime modifiedDateTime)
        {
            var plas = _repositoryPla.GetAll().ToList();
            var wgs = _repositoryWg.GetAll().ToList();

            var batchUpdate = new List<Wg>();

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_WG_START);
            var defaultCentralContractGroup = this._centralContractGroupRepo.GetAll().FirstOrDefault(ccg => ccg.Code == "NA");

            foreach (var item in items)
            {
                var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.WgCode));
                switch (item.Action.ToLower())
                {
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

                            _logger.Log(LogLevel.Debug, ImportConstants.NEW_WG, item.WgCode);
                            var newWg = new Wg
                            {
                                CreatedDateTime = DateTime.Now,
                                Description = item.WgDescription,
                                PlaId = item.PlaId.Value,
                                CompanyId = pla.CompanyId,
                                CentralContractGroupId = defaultCentralContractGroup?.Id,
                                Name = item.WgCode,
                                ExistsInLogisticsDb = true,
                                WgType = item.IsMultiVendor ? WgType.MultiVendor : WgType.Logistics,
                                ModifiedDateTime = DateTime.Now,
                                IsSoftware = false,
                                IsDeactivatedInLogistic = false
                            };
                            batchUpdate.Add(newWg);
                            this._newlyAddedWgs.Add(newWg);
                        }

                        //if WG is already exist in SCD Database
                        else
                        {
                            wg.IsDeactivatedInLogistic = false;
                            wg.DeactivatedDateTime = null;
                            wg.ModifiedDateTime = modifiedDateTime;
                            wg.ExistsInLogisticsDb = true;

                            //IF WG is Multi Vendor or Logistics update description and PLA
                            if (wg.WgType == WgType.MultiVendor || wg.WgType == WgType.Logistics)
                            {
                                wg.PlaId = pla.Id;
                                wg.Description = item.WgDescription;
                                wg.CompanyId = pla.CompanyId;
                            }
                            _logger.Log(LogLevel.Debug, ImportConstants.UPDATE_WG, item.WgCode);
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
                            if (wg.WgType == WgType.MultiVendor || wg.WgType == WgType.Logistics)
                                wg.DeactivatedDateTime = modifiedDateTime;
                            _logger.Log(LogLevel.Debug, ImportConstants.DEACTIVATE_START, item.WgCode);
                            batchUpdate.Add(wg);
                            this._deletedWgs.Add(wg.Id);
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
            var result = 0;
            var wgs = _repositoryWg.GetAll().Where(wg => !wg.DeactivatedDateTime.HasValue && !wg.IsSoftware).ToList();
            var allAvFees = _availabilityFeeRepo.GetAll().ToList();

            foreach (var item in items)
            {
                switch (item.Action.ToLower())
                {
                    case "u":
                        var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.WgCode, StringComparison.OrdinalIgnoreCase));
                        if (wg != null)
                        {
                            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_AVAILABILITY_FEE_START, wg.Name);
                           
                            var itemsToUpdate = allAvFees.Where(af => af.WgId == wg.Id && !af.DeactivatedDateTime.HasValue).ToList();
                            var batchList = new List<AvailabilityFeeWg>();
                            foreach (var itemToUpdate in itemsToUpdate)
                            {
                                if (item.IsJapanCostPerKit.ToLower() == "y")
                                    itemToUpdate.CostPerKitJapanBuy = item.CostPerKit;
                                else
                                    itemToUpdate.CostPerKit = item.CostPerKit;
                                itemToUpdate.MaxQty = item.MaxQty;
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
                        var deactivatedWg = _repositoryWg.GetAll().FirstOrDefault(w => w.Name.ToUpper() == item.WgCode.ToUpper());
                        if (deactivatedWg != null && deactivatedWg.WgType != WgType.Por && deactivatedWg.DeactivatedDateTime.HasValue)
                        {
                            _logger.Log(LogLevel.Debug, ImportConstants.DEACTIVATING_AVAILABILITY_FEE, deactivatedWg.Name);
                            var itemsToDeactivate = _availabilityFeeRepo.GetAll().Where(af => af.WgId == deactivatedWg.Id).ToList();
                            foreach(var deactivatedItem in itemsToDeactivate)
                            {
                                if (!deactivatedItem.DeactivatedDateTime.HasValue)
                                {
                                    deactivatedItem.DeactivatedDateTime = DateTime.Now;
                                    deactivatedItem.ModifiedDateTime = DateTime.Now;
                                }
                            }
                            _availabilityFeeRepo.Save(itemsToDeactivate);
                            _repositorySet.Sync();
                        }
                        break;
                }
            }

            return result;
        }
    }
}
