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

        protected PorService por;

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
            this.por = por;
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

            por.UploadSogs(step, sogsToUpload);
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

            this.newWgs = por.UploadWgs(step, wgsToUpload);
            step++;
        }


        protected virtual void UploadSwDigit()
        {
            var porSoftware = friese.GetSw();
            var swInfo = FormatDataHelper.FillSwInfo(porSoftware);
            var (rebuildRelationships, addedDigits) = por.UploadSoftwareDigits(porSoftware, swInfo, step);
            this.newDigits = addedDigits;
            step++;

            //STEP 4: UPLOAD SOFTWARE LICENCE
            var swLicensesInfo = swInfo.SwLicenses.Select(sw => sw.Value).ToList();
            rebuildRelationships = rebuildRelationships && por.UploadSoftwareLicense(swLicensesInfo, step);
            step++;

            //STEP 5: REBUILD RELATIONSHIPS BETWEEN SOFTWARE LICENSES AND DIGITS
            if (rebuildRelationships)
            {
                por.RebuildSoftwareInfo(porSoftware, step);
                step++;
            }
        }

        protected virtual void UploadFsp()
        {
            var countries = FormatDataHelper.FillCountryDictionary(por.CountryService.GetAll().ToList(), por.CountryGroupService.GetAll().ToList());

            var wgs = por.WgDomainService.GetAllActive().Where(wg => wg.WgType == Scd.Core.Enums.WgType.Por).ToList();
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
                    Proactive = por.GetSlasDictionaries().Proactive,
                    Sogs = por.GetSog(),
                    Wgs = wgs
                },
                Sla = por.GetSlasDictionaries(),
                OtherHardwareServiceTypes = hardwareServiceTypes,
                ProactiveServiceTypes = proactiveServiceTypes,
                StandardWarrantiesServiceTypes = standardWarrantiesServiceTypes
            };

            //UPLOAD HARDWARE
            por.UploadHwFspCodes(hwModel, step);
            step++;
        }

        protected virtual void UploadSwProactiveDigit()
        {
            var proActiveDigitModel = new SwProActiveDto
            {
                Proactive = por.GetSlasDictionaries().Proactive,
                SwDigits = por.GetDigits(),
                ProActiveInfo = friese.GetSwProactive(),
                CreatedDateTime = DateTime.Now
            };

            por.UploadSwProactiveInfo(proActiveDigitModel, step);
            step++;
        }

        protected virtual void UploadSw()
        {
            var proActiveDigits = por.ProActiveDigitService.GetAll().ToList();
            var license = por.LicenseService.GetAll().ToList();

            var swModel = new SwFspCodeDto
            {
                Sla = por.GetSlasDictionaries(),
                Digits = por.GetDigits(),
                SoftwareInfo = friese.GetSw(),
                SoftwareCodes = friese.GetSwFsp(),
                Sogs = por.GetSog(),
                SoftwareServiceTypes = softwareServiceTypes,
                CreatedDateTime = DateTime.Now,
                ProActiveDigits = proActiveDigits,
                License = license
            };

            por.UploadSwFspCodes(swModel, step);
            step++;
        }

        protected virtual void UpdateCostBlocks()
        {
            por.UpdateCostBlocks(step, por.UpdateQueryOptions);
            step++;
        }

        protected virtual void UpdateServiceSupport()
        {
            por.Update2ndLevelSupportCosts(step);
            por.ActivateProActiveSw(step);
            step++;
        }

        protected virtual void UpdateHwCosts()
        {
            por.UpdateCostBlocksByPla(step, this.newWgs);
            step++;
        }

        protected virtual void UpdateSwCosts()
        {
            por.UpdateCostBlocksBySog(step, this.newDigits);
            step++;
        }
    }
}
