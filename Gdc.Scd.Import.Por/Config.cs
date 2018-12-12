using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

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

        public static string[] AllServiceTypes
        {
            get
            {
                return HwServiceTypes
                        .Union(ProActiveServices)
                        .Union(SoftwareSolutionTypes)
                        .ToArray();
            }
        }
    }
}
