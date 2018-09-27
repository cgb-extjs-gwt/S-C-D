using Gdc.Scd.BusinessLogicLayer.Import;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Comparators;
using Ninject;
using Gdc.Scd.DataAccessLayer.External.Interfaces;
using Gdc.Scd.DataAccessLayer.External.Por;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Import.Por.Models;

namespace Gdc.Scd.Import.Por
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel(new Module());
            var logger = kernel.Get<ILogger<LogLevel>>();

            try
            {
                #region Initialize Services
                //services  
                var sogImporter = kernel.Get<IDataImporter<SCD2_ServiceOfferingGroups>>();
                var wgImporter = kernel.Get<IDataImporter<SCD2_WarrantyGroups>>();
                var softwareImporter = kernel.Get<IDataImporter<SCD2_SW_Overview>>();
                var fspCodesImporter = kernel.Get<IDataImporter<SCD2_v_SAR_new_codes>>();


                var plaService = kernel.Get<DomainService<Pla>>();

                //SLA ATOMS
                var locationService = kernel.Get<DomainService<ServiceLocation>>();
                var reactionTypeService = kernel.Get<DomainService<ReactionType>>();
                var responseTimeService = kernel.Get<DomainService<ReactionType>>();
                var availabilityService = kernel.Get<DomainService<Availability>>();
                var durationService = kernel.Get<DomainService<Duration>>();
                var proactiveService = kernel.Get<DomainService<ProActiveSla>>();
                var countryService = kernel.Get<DomainService<Country>>();
                var countryGroupService = kernel.Get<DomainService<CountryGroup>>();

                var sFabDomainService = kernel.Get<ImportService<SFab>>();
                var sogDomainService = kernel.Get<ImportService<Sog>>();
                var wgDomainService = kernel.Get<ImportService<Wg>>();
                var digitService = kernel.Get<ImportService<SwDigit>>();
                var licenseService = kernel.Get<ImportService<SwLicense>>();

                //SERVICES
                var sFabService = kernel.Get<IPorSFabsService>();
                var sogService = kernel.Get<IPorSogService>();
                var wgService = kernel.Get<IPorWgService>();
                var swDigitService = kernel.Get<IPorSwDigitService>();
                var swLicenseService = kernel.Get<IPorSwLicenseService>();
                var swLicenseDigitService = kernel.Get<IPorSwDigitLicenseService>();
                var hardwareService = kernel.Get<IHwFspCodeTranslationService>();
                var softwareService = kernel.Get<ISwFspCodeTranslationService>();
                #endregion

                

                //CONFIGURATION
                logger.Log(LogLevel.Info, "Reading configuration...");
                var softwareServiceTypes = Config.SoftwareSolutionTypes;
                var proactiveServiceTypes = Config.ProActiveServices;
                var standardWarrantiesServiceTypes = Config.StandardWarrantyTypes;
                var hardwareServiceTypes = Config.HwServiceTypes;
                var allowedServiceTypes = Config.AllServiceTypes;
                logger.Log(LogLevel.Info, "Reading configuration is completed.");


                //Start Process
                logger.Log(LogLevel.Info, ImportConstantMessages.START_PROCESS);

                Func<SCD2_ServiceOfferingGroups, bool> sogPredicate = sog => sog.Warranty_Calculation_relevant == "JA";
                Func<SCD2_WarrantyGroups, bool> wgPredicate = wg => wg.Warranty_Calculation_relevant == "JA";

                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_START, nameof(Sog));
                var porSogs = sogImporter.ImportData()
                    .Where(sogPredicate)
                    .ToList();

                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_ENDS, nameof(Sog), porSogs.Count);

                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_START, nameof(Wg));
                var porWGs = wgImporter.ImportData()
                    .Where(wgPredicate)
                    .ToList();
                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_ENDS, nameof(Wg), porWGs.Count);

                var plas = plaService.GetAll().ToList();
                bool success = false;
                int step = 1;

                //STEP 1: UPLOADING SFABs
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(SFab));

                var sfabDictionary = FillSFabDictionary(porSogs, porWGs);

                success = sFabService.UploadSFabs(sfabDictionary, plas, DateTime.Now);
                if (success)
                    success = sFabService.DeactivateSFabs(sfabDictionary, DateTime.Now);

                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP 2: UPLOADING SOGs
                var sFabs = sFabDomainService.GetAllActive().ToList();

                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(Sog));
                success = sogService.UploadSogs(porSogs, plas, sFabs, DateTime.Now);
                if (success)
                    success = sogService.DeactivateSogs(porSogs, DateTime.Now);
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP 3: UPLOAD WGs
                var sogs = sogDomainService.GetAllActive().ToList();

                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(Wg));
                success = wgService.UploadWgs(porWGs, sFabs, sogs, plas, DateTime.Now);
                if (success)
                    success = wgService.DeactivateSogs(porWGs, DateTime.Now);
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP 4: UPLOAD SOFTWARE DIGITS 
                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_START, "Software Info");
                var porSoftware = softwareImporter.ImportData()
                    .Where(sw => sw.Service_Code_Status == "50" && sw.SCD_Relevant == "x")
                    .ToList();
                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_ENDS, "Software Info", porSoftware.Count);

                var rebuildRelationships = true;

                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(SwDigit));
                var swInfo = FillSwInfo(porSoftware);
                success = swDigitService.UploadSwDigits(swInfo.SwDigits, sogs, DateTime.Now);
                rebuildRelationships = success;
                if (success)
                {
                    success = swDigitService.Deactivate(swInfo.SwDigits, DateTime.Now);
                    rebuildRelationships = rebuildRelationships && success;
                }
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP 5: UPLOAD SOFTWARE LICENCE
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(SwLicense));
                var swLicensesInfo = swInfo.SwLicenses.Select(sw => sw.Value).ToList();
                success = swLicenseService.UploadSwLicense(swLicensesInfo, DateTime.Now);
                rebuildRelationships = rebuildRelationships && success;
                if (success)
                {
                    success = swLicenseService.Deactivate(swLicensesInfo, DateTime.Now);
                    rebuildRelationships = rebuildRelationships && success;
                }
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP6: REBUILD RELATIONSHIPS BETWEEN SOFTWARE LICENSES AND DIGITS
                if (rebuildRelationships)
                {
                    logger.Log(LogLevel.Info, ImportConstantMessages.REBUILD_RELATIONSHIPS_START, step);
                    var licenses = licenseService.GetAllActive().ToList();
                    var digits = digitService.GetAllActive().ToList();
                    success = swLicenseDigitService.UploadSwDigitAndLicenseRelation(licenses, digits, porSoftware, DateTime.Now);
                    if (!success)
                    {
                        logger.Log(LogLevel.Warn, ImportConstantMessages.REBUILD_FAILS, step);
                    }
                    logger.Log(LogLevel.Info, ImportConstantMessages.REBUILD_RELATIONSHIPS_ENDS, step);
                    step++;
                }
                

                //STEP 7: UPLOAD FSP CODES AND TRANSLATIONS
                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_START, "FSP codes Translation");

                var locationServiceValues = locationService.GetAll().ToList();
                var reactionTypeValues = reactionTypeService.GetAll().ToList();
                var reactonTimeValues = responseTimeService.GetAll().ToList();
                var availabilityValues = availabilityService.GetAll().ToList();
                var durationValues = durationService.GetAll().ToList();
                var proActiveValues = proactiveService.GetAll().ToList();
                var countryValues = countryService.GetAll().ToList();

               

                var fspcodes = fspCodesImporter.ImportData()
                                               .Where(fsp => fsp.VStatus == "50" &&
                                                             allowedServiceTypes.Contains(fsp.SCD_ServiceType))
                                               .ToList();


                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_ENDS, "FSP codes Translation", fspcodes.Count);

                var result = FillCountryDictionary(countryService.GetAll().ToList(), countryGroupService.GetAll().ToList());
                var locationDictionary = FillSlaDictionary(locationServiceValues);
                var reactionTimeDictionary = FillSlaDictionary(reactionTypeValues);
                var rectionTypeDictionary = FillSlaDictionary(reactionTypeValues);
                var availabilityDictionary = FillSlaDictionary(availabilityValues);
                var durationDictionary = FillSlaDictionary(durationValues);
                var proactiveDictionary = FillSlaDictionary(proActiveValues);


                List<SCD2_v_SAR_new_codes> otherHardwareCodes = new List<SCD2_v_SAR_new_codes>();
                List<SCD2_v_SAR_new_codes> stdwCodes = new List<SCD2_v_SAR_new_codes>();
                List<SCD2_v_SAR_new_codes> proActiveCodes = new List<SCD2_v_SAR_new_codes>();
                List<SCD2_v_SAR_new_codes> softwareCodes = new List<SCD2_v_SAR_new_codes>();

                foreach (var code in fspcodes)
                {
                    if (hardwareServiceTypes.Contains(code.SCD_ServiceType))
                        otherHardwareCodes.Add(code);

                    else if (proactiveServiceTypes.Contains(code.SCD_ServiceType))
                        proActiveCodes.Add(code);

                    else if (standardWarrantiesServiceTypes.Contains(code.SCD_ServiceType))
                    {
                        if (code.SCD_ServiceType.Substring(11, 4).ToUpper() == "STDW")
                            stdwCodes.Add(code);
                    }

                    else if (softwareServiceTypes.Contains(code.SCD_ServiceType))
                        softwareCodes.Add(code);
                }

                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(HwFspCodeTranslation));
                
            }

            catch(Exception ex)
            {
                logger.Log(LogLevel.Fatal, ex, "POR Import completed unsuccessfully. Please find details below.");
                //TODO: Send emails
            }
        }

        #region Helper Methods
        /// <summary>
        /// Fill dictionary with SFab name and associated PLA
        /// </summary>
        /// <param name="sogs">Friese Sogs</param>
        /// <param name="wgs">Friese WGs</param>
        /// <returns></returns>
        private static Dictionary<string, string> FillSFabDictionary(
            IEnumerable<SCD2_ServiceOfferingGroups> sogs,
            IEnumerable<SCD2_WarrantyGroups> wgs)
        {
            var porFabsDictionary = new Dictionary<string, string>();

            foreach (var sog in sogs)
            {
                if (!porFabsDictionary.Keys.Contains(sog.FabGrp, StringComparer.OrdinalIgnoreCase))
                {
                    porFabsDictionary.Add(sog.FabGrp, sog.SOG_PLA);
                }
            }

            foreach (var wg in wgs)
            {
                if (!porFabsDictionary.Keys.Contains(wg.FabGrp, StringComparer.OrdinalIgnoreCase))
                {
                    porFabsDictionary.Add(wg.FabGrp, wg.Warranty_PLA);
                }
            }

            return porFabsDictionary;
        }

        private static SwHelperModel FillSwInfo(
            IEnumerable<SCD2_SW_Overview> swInfo)
        {
            var swDigitsDictionary = new Dictionary<string, string>();
            var swLicenseDictionary = new Dictionary<string, SCD2_SW_Overview>();

            foreach (var sw in swInfo)
            {
                if (!swDigitsDictionary.Keys.Contains(sw.Software_Lizenz_Digit, StringComparer.OrdinalIgnoreCase))
                    swDigitsDictionary.Add(sw.Software_Lizenz_Digit, sw.SOG_Code);

                if (!swLicenseDictionary.Keys.Contains(sw.Software_Lizenz, StringComparer.OrdinalIgnoreCase))
                    swLicenseDictionary.Add(sw.Software_Lizenz, sw);  
            }

            return new SwHelperModel(swDigitsDictionary, swLicenseDictionary);
        }


        private static Dictionary<string, List<long>> FillCountryDictionary(IEnumerable<Country> countries, 
            IEnumerable<CountryGroup> countryGroups)
        {
            var result = new Dictionary<string, List<long>>();
            foreach (var countryGroup in countryGroups)
            {
                if (String.IsNullOrEmpty(countryGroup.CountryDigit) && String.IsNullOrEmpty(countryGroup.LUTCode))
                    continue;


                var digits = countryGroup.LUTCode.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim())
                                         .Union(countryGroup.CountryDigit.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()));

                var masterCountries = countries.Where(c => c.CountryGroupId == countryGroup.Id && c.IsMaster);
                if (masterCountries.Any())
                {
                    foreach (var digit in digits)
                    {
                        if (result.Keys.Contains(digit))
                            result[digit].AddRange(masterCountries.Select(c => c.Id));
                        else
                            result.Add(digit, new List<long>(masterCountries.Select(c => c.Id)));
                        
                    }
                }
                
            }

            return result;
        }


        private static Dictionary<string, long> FillSlaDictionary<T>(IEnumerable<T> slas) where T : ExternalEntity
        {
            var result = new Dictionary<string, long>();
            foreach (var sla in slas)
            {
                var values = sla.ExternalName.Split(';').Select(s => s.Trim());
                foreach (var val in values)
                {
                    result.Add(val, sla.Id);
                }
            }

            return result;
        }
        #endregion
    }
}
