using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Impl;
using Gdc.Scd.Import.Por.Core.Interfaces;
using Gdc.Scd.Import.Por.Models;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por
{
    public class PorService
    {
        private ILogger Logger;

        public IDataImporter<SCD2_ServiceOfferingGroups> SogImporter { get; }
        public IDataImporter<SCD2_WarrantyGroups> WgImporter { get; }
        public IDataImporter<SCD2_SW_Overview> SoftwareImporter { get; }
        public IDataImporter<SCD2_v_SAR_new_codes> FspCodesImporter { get; }
        public IDataImporter<SCD2_LUT_TSP> LutCodesImporter { get; }
        public IDataImporter<SCD2_SWR_Level> SwProActiveImporter { get; }
        public DomainService<Pla> PlaService { get; }
        public DomainService<ServiceLocation> LocationService { get; }
        public DomainService<ReactionType> ReactionTypeService { get; }
        public DomainService<ReactionTime> ReactionTimeService { get; }
        public DomainService<Availability> AvailabilityService { get; }
        public DomainService<Duration> DurationService { get; }
        public DomainService<ProActiveSla> ProactiveService { get; }
        public DomainService<Country> CountryService { get; }
        public DomainService<CountryGroup> CountryGroupService { get; }
        public ImportService<SFab> SFabDomainService { get; }
        public ImportService<Sog> SogDomainService { get; }
        public ImportService<Wg> WgDomainService { get; }
        public ImportService<SwDigit> DigitService { get; }
        public ImportService<SwLicense> LicenseService { get; }
        public DomainService<ProActiveDigit> ProActiveDigitService { get; }
        public DomainService<SwSpMaintenance> SwSpMaintenanceDomainService { get; }
        public IPorSogService SogService { get; }
        public IPorWgService WgService { get; }
        public IPorSwDigitService SwDigitService { get; }
        public IPorSwSpMaintenaceService SwSpMaintenanceService { get; }
        public IPorSwLicenseService SwLicenseService { get; }
        public IPorSwDigitLicenseService SwLicenseDigitService { get; }
        public IHwFspCodeTranslationService<HwFspCodeDto> HardwareService { get; }
        public IHwFspCodeTranslationService<HwHddFspCodeDto> HardwareHddService { get; }
        public ISwFspCodeTranslationService SoftwareService { get; }
        public IPorSwProActiveService SoftwareProactiveService { get; }
        public ICostBlockService CostBlockService { get; }
        public List<UpdateQueryOption> UpdateQueryOptions { get; }

        public PorService(IKernel kernel)
        {
            Logger = kernel.Get<ILogger>();
            SogImporter = kernel.Get<IDataImporter<SCD2_ServiceOfferingGroups>>();
            WgImporter = kernel.Get<IDataImporter<SCD2_WarrantyGroups>>();
            SoftwareImporter = kernel.Get<IDataImporter<SCD2_SW_Overview>>();
            FspCodesImporter = kernel.Get<IDataImporter<SCD2_v_SAR_new_codes>>();
            LutCodesImporter = kernel.Get<IDataImporter<SCD2_LUT_TSP>>();
            PlaService = kernel.Get<DomainService<Pla>>();
            SwProActiveImporter = kernel.Get<IDataImporter<SCD2_SWR_Level>>();

            //SLA ATOMS
            LocationService = kernel.Get<DomainService<ServiceLocation>>();
            ReactionTypeService = kernel.Get<DomainService<ReactionType>>();
            ReactionTimeService = kernel.Get<DomainService<ReactionTime>>();
            AvailabilityService = kernel.Get<DomainService<Availability>>();
            DurationService = kernel.Get<DomainService<Duration>>();
            ProactiveService = kernel.Get<DomainService<ProActiveSla>>();
            CountryService = kernel.Get<DomainService<Country>>();
            CountryGroupService = kernel.Get<DomainService<CountryGroup>>();
            ProActiveDigitService = kernel.Get<DomainService<ProActiveDigit>>();
            SwSpMaintenanceDomainService = kernel.Get<DomainService<SwSpMaintenance>>();

            SFabDomainService = kernel.Get<ImportService<SFab>>();
            SogDomainService = kernel.Get<ImportService<Sog>>();
            WgDomainService = kernel.Get<ImportService<Wg>>();
            DigitService = kernel.Get<ImportService<SwDigit>>();
            LicenseService = kernel.Get<ImportService<SwLicense>>();


            //SERVICES
            SogService = kernel.Get<IPorSogService>();
            WgService = kernel.Get<IPorWgService>();
            SwDigitService = kernel.Get<IPorSwDigitService>();
            SwLicenseService = kernel.Get<IPorSwLicenseService>();
            SwLicenseDigitService = kernel.Get<IPorSwDigitLicenseService>();
            HardwareService = kernel.Get<IHwFspCodeTranslationService<HwFspCodeDto>>();
            HardwareHddService = kernel.Get<IHwFspCodeTranslationService<HwHddFspCodeDto>>();
            SoftwareService = kernel.Get<ISwFspCodeTranslationService>();
            SoftwareProactiveService = kernel.Get<IPorSwProActiveService>();
            CostBlockService = kernel.Get<ICostBlockService>();
            SwSpMaintenanceService = kernel.Get<IPorSwSpMaintenaceService>();

            UpdateQueryOptions = new List<UpdateQueryOption>();
        }


        public virtual void UploadSogs(List<Pla> plas, int step,
            List<SogPorDto> sogs)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(Sog));
            var success = SogService.UploadSogs(sogs, plas, DateTime.Now,
                UpdateQueryOptions);
            if (success)
                success = SogService.DeactivateSogs(sogs, DateTime.Now);
            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
        }


        public virtual void UploadWgs(List<Pla> plas, int step,
            List<Sog> sogs, List<WgPorDto> wgs)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(Wg));
            var success = WgService.UploadWgs(wgs, sogs, plas, DateTime.Now, UpdateQueryOptions);
            if (success)
                success = WgService.DeactivateWgs(wgs, DateTime.Now);
            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
        }


        public virtual bool UploadSoftwareDigits(List<SCD2_SW_Overview> porSoftware, List<Sog> sogs,
            SwHelperModel swInfo,
            int step)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(SwDigit));
            var success = SwDigitService.UploadSwDigits(swInfo.SwDigits, sogs, DateTime.Now, UpdateQueryOptions);
            if (success)
            {
                success = SwDigitService.Deactivate(swInfo.SwDigits, DateTime.Now);
            }
            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
            return success;
        }


        public virtual bool UploadSoftwareLicense(List<SCD2_SW_Overview> swLicensesInfo, int step)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(SwLicense));
            var success = SwLicenseService.UploadSwLicense(swLicensesInfo, DateTime.Now, UpdateQueryOptions);
            if (success)
            {
                success = SwLicenseService.Deactivate(swLicensesInfo, DateTime.Now);
            }
            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
            return success;
        }


        public virtual void RebuildSoftwareInfo(List<SwDigit> digits, IEnumerable<SCD2_SW_Overview> swInfodigits, int step)
        {
            Logger.Info(ImportConstantMessages.REBUILD_RELATIONSHIPS_START, step, nameof(SwDigit), nameof(SwLicense));
            var licenses = LicenseService.GetAllActive().ToList();
            var success = SwLicenseDigitService.UploadSwDigitAndLicenseRelation(licenses, digits, swInfodigits, DateTime.Now);
            if (!success)
            {
                Logger.Warn(ImportConstantMessages.REBUILD_FAILS, step);
            }
            Logger.Info(ImportConstantMessages.REBUILD_RELATIONSHIPS_END, step);
        }

        public virtual void UploadHwFspCodes(HwFspCodeDto model, int step)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(TempHwFspCodeTranslation));

            var success = HardwareService.UploadHardware(model);

            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(HwHddFspCodeTranslation));

            var hwHddDto = new HwHddFspCodeDto
            {
                HardwareCodes = model.HddRetentionCodes,
                CreationDate = model.CreationDate,
                HwSla = model.HwSla
            };

            var uploadHddSuccess = HardwareHddService.UploadHardware(hwHddDto);
            success = uploadHddSuccess && success;

            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
        }

        public virtual bool UploadSwProactiveInfo(SwProActiveDto model, int step)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, "Software Proactive Info");
            var success = SoftwareProactiveService.UploadSwProactiveInfo(model);
            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
            return success;
        }

        public virtual void UploadSwFspCodes(SwFspCodeDto model, int step)
        {
            Logger.Info(ImportConstantMessages.UPLOAD_START, step, nameof(SwFspCodeTranslation));

            var success = SoftwareService.UploadSoftware(model);

            Logger.Info(ImportConstantMessages.UPLOAD_ENDS, step);
        }

        public virtual void UpdateCostBlocks(int step, IEnumerable<UpdateQueryOption> updateOptions)
        {
            try
            {
                Logger.Info(ImportConstantMessages.UPDATE_COST_BLOCKS_START, step);
                CostBlockService.UpdateByCoordinates(updateOptions);
                Logger.Info(ImportConstantMessages.UPDATE_COST_BLOCKS_END);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ImportConstantMessages.UNEXPECTED_ERROR);
            }
        }

        public virtual void Update2ndLevelSupportCosts(int step)
        {
            try
            {
                Logger.Info(ImportConstantMessages.UPDATE_COSTS_START, step);

                SwSpMaintenanceService.Update2ndLevelSupportCosts(DigitService.GetAllActive().ToList(), SwSpMaintenanceDomainService.GetAll().ToList());

                Logger.Info(ImportConstantMessages.UPDATE_COSTS_END);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ImportConstantMessages.UNEXPECTED_ERROR);
            }
        }

        /// <summary>
        /// Fill SLA Dictionary
        /// </summary>
        /// <returns></returns>
        public virtual SlaDictsDto FillSlasDictionaries()
        {
            var locationServiceValues = this.LocationService.GetAll().ToList();
            var reactionTypeValues = this.ReactionTypeService.GetAll().ToList();
            var reactonTimeValues = this.ReactionTimeService.GetAll().ToList();
            var availabilityValues = this.AvailabilityService.GetAll().ToList();
            var durationValues = this.DurationService.GetAll().ToList();
            var proactiveValues = this.ProactiveService.GetAll().ToList();

            var locationDictionary = FormatDataHelper.FillSlaDictionary(locationServiceValues);
            var reactionTimeDictionary = FormatDataHelper.FillSlaDictionary(reactonTimeValues);
            var reactionTypeDictionary = FormatDataHelper.FillSlaDictionary(reactionTypeValues);
            var availabilityDictionary = FormatDataHelper.FillSlaDictionary(availabilityValues);
            var durationDictionary = FormatDataHelper.FillSlaDictionary(durationValues);
            var proactiveDictionary = FormatDataHelper.FillSlaDictionary(proactiveValues);

            return new SlaDictsDto
            {
                Availability = availabilityDictionary,
                Duration = durationDictionary,
                Locations = locationDictionary,
                ReactionTime = reactionTimeDictionary,
                ReactionType = reactionTypeDictionary,
                Proactive = proactiveDictionary
            };
        }
    }
}
