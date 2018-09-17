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

namespace Gdc.Scd.Import.Por
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel(new Module());

            //services
            var logger = kernel.Get<ILogger<LogLevel>>();

            //TODO: Replace with one binding when Dirk gives an access
            var sogImporter = kernel.Get<IDataImporter<Intranet_SOG_Info>>("por");
            var wgImporter = kernel.Get<IDataImporter<Intranet_WG_Info>>("por");
            var softwareImporter = kernel.Get<IDataImporter<SCD_SW_Overview>>("oldPor");


            var plaService = kernel.Get<DomainService<Pla>>();
            var sFabDomainService = kernel.Get<DomainService<SFab>>();
            var sogDomainService = kernel.Get<DomainService<Sog>>();
            var wgDomainService = kernel.Get<DomainService<Wg>>();

            //SERVICES
            var sFabService = kernel.Get<IPorSFabsService>();
            var sogService = kernel.Get<IPorSogService>();
            var wgService = kernel.Get<IPorWgService>();
            var swDigitService = kernel.Get<IPorSwDigitService>();

            //Start Process
            logger.Log(LogLevel.Info, "Process started...");
            logger.Log(LogLevel.Info, "Getting SOGs from POR...");

            Func<Intranet_SOG_Info, bool> sogPredicate = sog => sog.SCD_relevant == "Yes" && sog.Activ == "Yes";
            Func<Intranet_WG_Info, bool> wgPredicate = wg => wg.SCD_relevant == "Yes" && wg.Activ == "Yes";

            var porSogs = sogImporter.ImportData()
                .Where(sogPredicate)
                .ToList();

            var porWGs = wgImporter.ImportData()
                .Where(wgPredicate)
                .ToList();

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
            var sFabs = sFabDomainService.GetAll().Where(sFab => !sFab.DeactivatedDateTime.HasValue).ToList();

            logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(Sog));
            success = sogService.UploadSogs(porSogs, plas, sFabs, DateTime.Now);
            if (success)
                success = sogService.DeactivateSogs(porSogs, DateTime.Now);
            logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
            step++;

            //STEP 3: UPLOAD WGs
            var sogs = sogDomainService.GetAll().Where(sog => !sog.DeactivatedDateTime.HasValue).ToList();

            logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(Wg));
            success = wgService.UploadWgs(porWGs, sFabs, sogs, plas, DateTime.Now);
            if (success)
                success = wgService.DeactivateSogs(porWGs, DateTime.Now);
            logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
            step++;

            //STEP 4: UPLOAD SOFTWARE DIGITS
            var porSoftware = softwareImporter.ImportData()
                .Where(sw => sw.Service_Code_Status == "50")
                .ToList();

            logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_START, step, nameof(SwDigit));
            var swDictionary = FillSwDigitDictionary(porSoftware);
            success = swDigitService.UploadSwDigits(swDictionary, sogs, DateTime.Now);
            if (success)
                success = swDigitService.Deactivate(swDictionary, DateTime.Now);
            logger.Log(LogLevel.Info, ImportConstantMessages.UPLOAD_ENDS, step);
            step++;

            //STEP 5: UPLOAD SOFTWARE LICENCE
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

        private static Dictionary<string, string> FillSwDigitDictionary(
            IEnumerable<SCD_SW_Overview> swInfo)
        {
            var swDictionary = new Dictionary<string, string>();
            foreach (var sw in swInfo)
            {
                if (!swDictionary.Keys.Contains(sw.Software_Lizenz_Digit, StringComparer.OrdinalIgnoreCase))
                    swDictionary.Add(sw.Software_Lizenz_Digit, sw.SOG_Code);
            }

            return swDictionary;
        }
        #endregion
    }
}
