using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por
{
    public class FrieseClient
    {
        private List<SCD2_v_SAR_new_codes> otherHardwareCodes;
        private List<SCD2_v_SAR_new_codes> stdwCodes;
        private List<SCD2_v_SAR_new_codes> proActiveCodes;
        private List<SCD2_v_SAR_new_codes> hddRetentionCodes;
        private List<SCD2_v_SAR_new_codes> softwareCodes;
        private List<SCD2_LUT_TSP> lutCodes;

        private List<SCD2_ServiceOfferingGroups> porSogs;
        private List<SCD2_WarrantyGroups> porWGs;
        private List<SCD2_SW_Overview> porSoftware;
        private List<SCD2_SWR_Level> swProActive;

        private string[] softwareServiceTypes;
        private string[] proactiveServiceTypes;
        private string[] standardWarrantiesServiceTypes;
        private string[] hardwareServiceTypes;
        private string[] allowedServiceTypes;
        private string[] hddServiceTypes;

        private bool fspPrepared;

        private FrieseEntities _frieseEntities;

        private ILogger log;

        public FrieseClient(ILogger log)
        {
            this._frieseEntities = new FrieseEntities();
            this.log = log;
            this.fspPrepared = false;
        }

        public virtual List<SCD2_ServiceOfferingGroups> GetSog()
        {
            if (porSogs == null)
            {
                log.Info(ImportConstantMessages.FETCH_INFO_START, nameof(Sog));
                porSogs = _frieseEntities.SCD2_ServiceOfferingGroups.ToList();
                log.Info(ImportConstantMessages.FETCH_INFO_ENDS, nameof(Sog), porSogs.Count);
            }
            return porSogs;
        }

        public virtual List<SCD2_WarrantyGroups> GetWg()
        {
            if (porWGs == null)
            {
                log.Info(ImportConstantMessages.FETCH_INFO_START, nameof(Wg));
                porWGs = _frieseEntities.SCD2_WarrantyGroups.ToList();
                log.Info(ImportConstantMessages.FETCH_INFO_ENDS, nameof(Wg), porWGs.Count);
            }
            return porWGs;
        }

        public virtual List<SCD2_SW_Overview> GetSw()
        {
            if (porSoftware == null)
            {
                log.Info(ImportConstantMessages.FETCH_INFO_START, "Software Info");
                porSoftware = _frieseEntities.SCD2_SW_Overview
                                                    .Where(sw => sw.Service_Code_Status == "50" && sw.SCD_Relevant == "x")
                                                    .ToList();
                log.Info(ImportConstantMessages.FETCH_INFO_ENDS, "Software Info", porSoftware.Count);
            }
            return porSoftware;
        }

        public virtual List<SCD2_SWR_Level> GetSwProactive()
        {
            if (swProActive == null)
            {
                log.Info(ImportConstantMessages.FETCH_INFO_START, "Software ProActive");
                swProActive = _frieseEntities.SCD2_SWR_Level.ToList();
                log.Info(ImportConstantMessages.FETCH_INFO_ENDS, "Software ProActive", swProActive.Count);
            }
            return swProActive;
        }

        public virtual List<SCD2_v_SAR_new_codes> GetStdwFsp()
        {
            if (!fspPrepared)
            {
                PrepareFsp();
            }
            return stdwCodes;
        }

        public virtual List<SCD2_v_SAR_new_codes> GetOtherHardwareFsp()
        {
            if (!fspPrepared)
            {
                PrepareFsp();
            }
            return otherHardwareCodes;
        }

        public virtual List<SCD2_v_SAR_new_codes> GetProActiveFsp()
        {
            if (!fspPrepared)
            {
                PrepareFsp();
            }
            return proActiveCodes;
        }

        public virtual List<SCD2_v_SAR_new_codes> GetHddFsp()
        {
            if (!fspPrepared)
            {
                PrepareFsp();
            }
            return hddRetentionCodes;
        }

        public virtual List<SCD2_v_SAR_new_codes> GetSwFsp()
        {
            if (!fspPrepared)
            {
                PrepareFsp();
            }
            return softwareCodes;
        }

        public virtual List<SCD2_LUT_TSP> GetLut()
        {
            if (!fspPrepared)
            {
                PrepareFsp();
            }
            return lutCodes;
        }

        protected virtual void PrepareFsp()
        {
            //CONFIGURATION
            log.Info("Friese. Reading configuration...");
            softwareServiceTypes = Config.SoftwareSolutionTypes;
            proactiveServiceTypes = Config.ProActiveServices;
            standardWarrantiesServiceTypes = Config.StandardWarrantyTypes;
            hardwareServiceTypes = Config.HwServiceTypes;
            allowedServiceTypes = Config.AllServiceTypes;
            hddServiceTypes = Config.HddServiceType;
            log.Info("Friese. Reading configuration is completed.");


            log.Info(ImportConstantMessages.FETCH_INFO_START, "FSP codes Translation");
            var fspcodes = LoadFsp();
            log.Info(ImportConstantMessages.FETCH_INFO_ENDS, "FSP codes Translation", fspcodes.Count);


            otherHardwareCodes = new List<SCD2_v_SAR_new_codes>();
            stdwCodes = new List<SCD2_v_SAR_new_codes>();
            proActiveCodes = new List<SCD2_v_SAR_new_codes>();
            hddRetentionCodes = new List<SCD2_v_SAR_new_codes>();
            softwareCodes = new List<SCD2_v_SAR_new_codes>();

            foreach (var code in fspcodes)
            {
                if (hardwareServiceTypes.Contains(code.SCD_ServiceType))
                    otherHardwareCodes.Add(code);

                else if (proactiveServiceTypes.Contains(code.SCD_ServiceType))
                    proActiveCodes.Add(code);

                else if (standardWarrantiesServiceTypes.Contains(code.SCD_ServiceType))
                {
                    stdwCodes.Add(code);
                }

                else if (softwareServiceTypes.Contains(code.SCD_ServiceType))
                    softwareCodes.Add(code);

                else if (hddServiceTypes.Contains(code.SCD_ServiceType))
                    hddRetentionCodes.Add(code);
            }

            log.Info(ImportConstantMessages.FETCH_INFO_START, "Standard Warranties");
            lutCodes = LoadLut();
            log.Info(ImportConstantMessages.FETCH_INFO_ENDS, "Standard Warranties", lutCodes.Count);

            //
            fspPrepared = true;
        }

        protected virtual List<SCD2_v_SAR_new_codes> LoadFsp()
        {
            //VStatus is ignored for STDWs 
            return _frieseEntities.SCD2_v_SAR_new_codes
                                           .Where(fsp => (fsp.VStatus == "50" &&
                                                         allowedServiceTypes.Contains(fsp.SCD_ServiceType)) ||
                                                         (standardWarrantiesServiceTypes.Contains(fsp.SCD_ServiceType)
                                                         && (fsp.Service_Code.Substring(11, 4).ToUpper().Equals("STDW") ||
                                                             fsp.Service_Code.Substring(11, 4).ToUpper().Equals("SMDW"))))
                                           .ToList();
        }

        protected virtual List<SCD2_LUT_TSP> LoadLut()
        {
            return _frieseEntities.SCD2_LUT_TSP.ToList();
        }
    }
}
