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
                //TODO: Replace with one binding when Dirk gives an access
                var sogImporter = kernel.Get<IDataImporter<Intranet_SOG_Info>>("por");
                var wgImporter = kernel.Get<IDataImporter<Intranet_WG_Info>>("por");
                var softwareImporter = kernel.Get<IDataImporter<SCD_SW_Overview>>("oldPor");
                var fspCodesImporter = kernel.Get<IDataImporter<v_SAR_new_codes>>("por");


                var plaService = kernel.Get<DomainService<Pla>>();

                //SLA ATOMS
                var locationService = kernel.Get<DomainService<ServiceLocation>>();
                var reactionTypeService = kernel.Get<DomainService<ReactionType>>();
                var responseTimeService = kernel.Get<DomainService<ReactionType>>();
                var availabilityService = kernel.Get<DomainService<Availability>>();
                var durationService = kernel.Get<DomainService<Duration>>();
                var proactiveService = kernel.Get<DomainService<ProActiveSla>>();

                var sFabDomainService = kernel.Get<ImportPorService<SFab>>();
                var sogDomainService = kernel.Get<ImportPorService<Sog>>();
                var wgDomainService = kernel.Get<ImportPorService<Wg>>();
                var digitService = kernel.Get<ImportPorService<SwDigit>>();

                //SERVICES
                var sFabService = kernel.Get<IPorSFabsService>();
                var sogService = kernel.Get<IPorSogService>();
                var wgService = kernel.Get<IPorWgService>();
                var swDigitService = kernel.Get<IPorSwDigitService>();
                var swLicenseService = kernel.Get<IPorSwLicenseService>();

                #endregion
                
                //CONFIGURATION
                logger.Log(LogLevel.Info, "Reading configuration...");
                var allServiceTypes = Config.AllServiceTypes;
                var proactiveServiceTypes = Config.ProActiveServices;
                var stw = Config.StandardWarrantyTypes;
                logger.Log(LogLevel.Info, "Reading configuration is completed.");


                //Start Process
                logger.Log(LogLevel.Info, ImportConstantMessages.START_PROCESS);

                Func<Intranet_SOG_Info, bool> sogPredicate = sog => sog.SCD_relevant == "Yes" && sog.Activ == "Yes";
                Func<Intranet_WG_Info, bool> wgPredicate = wg => wg.SCD_relevant == "Yes" && wg.Activ == "Yes";

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

                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(SwDigit));
                var swInfo = FillSwInfo(porSoftware);
                success = swDigitService.UploadSwDigits(swInfo.SwDigits, sogs, DateTime.Now);
                if (success)
                    success = swDigitService.Deactivate(swInfo.SwDigits, DateTime.Now);
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP 5: UPLOAD SOFTWARE LICENCE
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(SwLicense));
                var digits = digitService.GetAllActive().ToList();
                var swLicensesInfo = swInfo.SwLicenses.Select(sw => sw.Value).ToList();
                success = swLicenseService.UploadSwLicense(swLicensesInfo,
                    digits, DateTime.Now);
                if (success)
                    success = swLicenseService.Deactivate(swLicensesInfo, DateTime.Now);
                logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
                step++;

                //STEP 6: UPLOAD FSP CODES AND TRANSLATIONS
                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_START, nameof(FspCodeTranslation));

                var locationServiceValues = locationService.GetAll().ToList();
                var reactionTypeValues = reactionTypeService.GetAll().ToList();
                var reactonTimeValues = responseTimeService.GetAll().ToList();
                var availabilityValues = availabilityService.GetAll().ToList();
                var durationValues = durationService.GetAll().ToList();
                var proActiveValues = proactiveService.GetAll().ToList();

                var allowedServiceTypes = Config.AllServiceTypes;

                var fspcodes = fspCodesImporter.ImportData()
                                               .Where(fsp => fsp.VStatus == "50" && 
                                                             allowedServiceTypes.Contains(fsp.SCD_ServiceType))
                                               .ToList();


                logger.Log(LogLevel.Info, ImportConstantMessages.FETCH_INFO_ENDS, nameof(FspCodeTranslation));
                //logger.Log(LogLevel.Info)
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
            IEnumerable<Intranet_SOG_Info> sogs,
            IEnumerable<Intranet_WG_Info> wgs)
        {
            var porFabsDictionary = new Dictionary<string, string>();

            foreach (var sog in sogs)
            {
                if (!porFabsDictionary.Keys.Contains(sog.ServiceFabgr, StringComparer.OrdinalIgnoreCase))
                {
                    porFabsDictionary.Add(sog.ServiceFabgr, sog.Produktreihe);
                }
            }

            foreach (var wg in wgs)
            {
                if (!porFabsDictionary.Keys.Contains(wg.ServiceFabgr, StringComparer.OrdinalIgnoreCase))
                {
                    porFabsDictionary.Add(wg.ServiceFabgr, wg.PLA);
                }
            }

            return porFabsDictionary;
        }

        private static SwHelperModel FillSwInfo(
            IEnumerable<SCD_SW_Overview> swInfo)
        {
            var swDigitsDictionary = new Dictionary<string, string>();
            var swLicenseDictionary = new Dictionary<string, SCD_SW_Overview>();

            foreach (var sw in swInfo)
            {
                if (!swDigitsDictionary.Keys.Contains(sw.Software_Lizenz_Digit, StringComparer.OrdinalIgnoreCase))
                    swDigitsDictionary.Add(sw.Software_Lizenz_Digit, sw.SOG_Code);

                if (!swLicenseDictionary.Keys.Contains(sw.Software_Lizenz, StringComparer.OrdinalIgnoreCase))
                    swLicenseDictionary.Add(sw.Software_Lizenz, sw);  
            }

            return new SwHelperModel(swDigitsDictionary, swLicenseDictionary);
        }
        #endregion
    }
}
