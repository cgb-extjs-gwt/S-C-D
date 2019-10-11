using System;
using System.Configuration;
using System.Linq;

namespace Gdc.Scd.Import.Por
{
    public static class Config
    {
        public static string[] HwServiceTypes
        {
            get
            {
                var hwServiceTypes = ConfigurationManager.AppSettings["HWServiceTypes"];
                if (String.IsNullOrEmpty(hwServiceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "SCDServiceTypes"));
                return hwServiceTypes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] ProActiveServices
        {
            get
            {
                var proactiveServiceTypes = ConfigurationManager.AppSettings["ProActiveTypes"];
                if (String.IsNullOrEmpty(proactiveServiceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "ProActiveTypes"));
                return proactiveServiceTypes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] StandardWarrantyTypes
        {
            get
            {
                var stwServiceTypes = ConfigurationManager.AppSettings["StandardWarrantyTypes"];
                if (String.IsNullOrEmpty(stwServiceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "StandardWarrantyTypes"));
                return stwServiceTypes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] SoftwareSolutionTypes
        {
            get
            {
                var swServiceTypes = ConfigurationManager.AppSettings["SoftwareTypes"];
                if (String.IsNullOrEmpty(swServiceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "SoftwareTypes"));
                return swServiceTypes.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] HddServiceType
        {
            get
            {
                var hddServiceType = ConfigurationManager.AppSettings["HwHddServiceType"];
                if (String.IsNullOrEmpty(hddServiceType))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "SCDServiceTypes"));
                return hddServiceType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string SolutionIdentifier => 
            ConfigurationManager.AppSettings["SolutionIdentifier"];

        public static string[] AllServiceTypes =>
            HwServiceTypes
                .Union(ProActiveServices)
                .Union(SoftwareSolutionTypes)
                .Union(HddServiceType)
                .ToArray();

        public static string[] ExceptionalHardwareWgs
        {
            get
            {
                if (ConfigurationManager.AppSettings["ExceptionalHardwareWgs"] == null)
                    return new string[0];
                return ConfigurationManager.AppSettings["ExceptionalHardwareWgs"]
                    .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
