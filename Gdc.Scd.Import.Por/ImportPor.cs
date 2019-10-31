using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Por.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por
{
    public class ImportPor
    {
        protected ILogger log;

        protected PorService PorService;

        protected FrieseClient friese;

        protected List<Wg> newWgs;

        protected List<SwDigit> newDigits;

        protected string[] softwareServiceTypes;
        protected string[] proactiveServiceTypes;
        protected string[] standardWarrantiesServiceTypes;
        protected string[] hardwareServiceTypes;
        protected string[] allowedServiceTypes;
        protected string[] hddServiceTypes;

        protected int step;

        public ImportPor(PorService por, FrieseClient friese, ILogger log)
        {
            this.PorService = por;
            this.friese = friese;
            this.log = log;
        }

        protected ImportPor() { }

        public virtual void Run()
        {
            //CONFIGURATION
            log.Info("Reading configuration...");
            softwareServiceTypes = Config.SoftwareSolutionTypes;
            proactiveServiceTypes = Config.ProActiveServices;
            standardWarrantiesServiceTypes = Config.StandardWarrantyTypes;
            hardwareServiceTypes = Config.HwServiceTypes;
            allowedServiceTypes = Config.AllServiceTypes;
            hddServiceTypes = Config.HddServiceType;

            log.Info("Reading configuration is completed.");


            //Start Process
            log.Info(ImportConstantMessages.START_PROCESS);

            step = 1;

            //STEP 1: UPLOADING SOGs
            UploadSog();

            //STEP 2: UPLOAD WGs
            UploadWg();

            //STEP 3: UPLOAD SOFTWARE DIGITS 
            UploadSwDigit();

            //STEP 6: UPLOAD FSP CODES AND TRANSLATIONS
            UploadFsp();

            //STEP 7: PROACTIVE DIGITS UPLOAD
            UploadSwProactiveDigit();

            //STEP 8: UPLOAD SOFTWARE
            UploadSw();

            //STEP 9: UPLOAD COST BLOCKS
            UpdateCostBlocks();

            //STEP 10: UPDATE 2ndLevelSupportCosts
            UpdateServiceSupport();

            //STEP 11: UPDATE COST BLOCK ELEMENTS BY PLA
            UpdateHwCosts();

            //STEP 12: UPDATE SOFTWARE COST BLOCK ELEMENTS BY SOG
            UpdateSwCosts();

            log.Info(ImportConstantMessages.END_PROCESS);
        }

        protected virtual void UploadSog()
        {
            var sogsToUpload = new List<SogPorDto>();

            foreach (var porSog in friese.GetSog())
            {
                var sogDto = new SogPorDto(porSog, Config.SolutionIdentifier);
                if (!sogDto.IsSoftware || sogDto.IsSoftware && sogDto.ActivePorFlag)
                    sogsToUpload.Add(sogDto);
            }

            PorService.UploadSogs(step, sogsToUpload);
            step++;
        }

        protected virtual void UploadWg()
        {
            var wgsToUpload = new List<WgPorDto>();

            foreach (var porWg in friese.GetWg())
            {
                var wgDto = new WgPorDto(porWg);
                if (!wgDto.IsSoftware || wgDto.IsSoftware && wgDto.ActivePorFlag)
                    wgsToUpload.Add(wgDto);
            }

            this.newWgs = PorService.UploadWgs(step, wgsToUpload);
            step++;
        }

        protected virtual void UploadSwDigit()
        {
            var porSoftware = friese.GetSw();
            var swInfo = FormatDataHelper.FillSwInfo(porSoftware);
            var (rebuildRelationships, addedDigits) = PorService.UploadSoftwareDigits(porSoftware, swInfo, step);
            this.newDigits = addedDigits;
            step++;

            //STEP 4: UPLOAD SOFTWARE LICENCE
            var swLicensesInfo = swInfo.SwLicenses.Select(sw => sw.Value).ToList();
            rebuildRelationships = rebuildRelationships && PorService.UploadSoftwareLicense(swLicensesInfo, step);
            step++;

            //STEP 5: REBUILD RELATIONSHIPS BETWEEN SOFTWARE LICENSES AND DIGITS
            if (rebuildRelationships)
            {
                PorService.RebuildSoftwareInfo(porSoftware, step);
                step++;
            }
        }

        protected virtual void UploadFsp()
        {
            var countries = FormatDataHelper.FillCountryDictionary(PorService.CountryService.GetAll().ToList(), PorService.CountryGroupService.GetAll().ToList());

            var wgs = PorService.WgDomainService.GetAllActive().Where(wg => wg.WgType == Scd.Core.Enums.WgType.Por).ToList();
            var hwModel = new HwFspCodeDto
            {
                HardwareCodes = friese.GetOtherHardwareFsp(),
                ProactiveCodes = friese.GetProActiveFsp(),
                StandardWarranties = friese.GetStdwFsp(),
                HddRetentionCodes = friese.GetHddFsp(),
                LutCodes = friese.GetLut(),
                CreationDate = DateTime.Now,
                HwSla = new HwSlaDto
                {
                    Countries = countries,
                    Proactive = PorService.GetSlasDictionaries().Proactive,
                    Sogs = PorService.GetSog(),
                    Wgs = wgs
                },
                Sla = PorService.GetSlasDictionaries(),
                OtherHardwareServiceTypes = hardwareServiceTypes,
                ProactiveServiceTypes = proactiveServiceTypes,
                StandardWarrantiesServiceTypes = standardWarrantiesServiceTypes
            };

            //UPLOAD HARDWARE
            PorService.UploadHwFspCodes(hwModel, step);
            step++;
        }

        protected virtual void UploadSwProactiveDigit()
        {
            var proActiveDigitModel = new SwProActiveDto
            {
                Proactive = PorService.GetSlasDictionaries().Proactive,
                SwDigits = PorService.GetDigits(),
                ProActiveInfo = friese.GetSwProactive(),
                CreatedDateTime = DateTime.Now
            };

            PorService.UploadSwProactiveInfo(proActiveDigitModel, step);
            step++;
        }

        protected virtual void UploadSw()
        {
            var proActiveDigits = PorService.ProActiveDigitService.GetAll().ToList();
            var license = PorService.LicenseService.GetAll().ToList();

            var swModel = new SwFspCodeDto
            {
                Sla = PorService.GetSlasDictionaries(),
                Digits = PorService.GetDigits(),
                SoftwareInfo = friese.GetSw(),
                SoftwareCodes = friese.GetSwFsp(),
                Sogs = PorService.GetSog(),
                SoftwareServiceTypes = softwareServiceTypes,
                CreatedDateTime = DateTime.Now,
                ProActiveDigits = proActiveDigits,
                License = license
            };

            PorService.UploadSwFspCodes(swModel, step);
            step++;
        }

        protected virtual void UpdateCostBlocks()
        {
            PorService.UpdateCostBlocks(step, PorService.UpdateQueryOptions);
            step++;
        }

        protected virtual void UpdateServiceSupport()
        {
            PorService.Update2ndLevelSupportCosts(step);
            step++;
        }

        protected virtual void UpdateSwCosts()
        {
            PorService.UpdateCostBlocksBySog(step, this.newDigits);
        }

        protected virtual void UpdateHwCosts()
        {
            PorService.UpdateCostBlocksByPla(step, this.newWgs);
            step++;
        }
    }
}
