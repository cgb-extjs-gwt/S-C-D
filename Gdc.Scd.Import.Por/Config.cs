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
        public static string[] AllowedServiceTypes
        {
            get
            {
                var serviceTypes = ConfigurationManager.AppSettings["SCDServiceTypes"];
                if (String.IsNullOrEmpty(serviceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "SCDServiceTypes"));
                return serviceTypes.Split(';');
            }
        }

        public static string[] ProActiveServices
        {
            get
            {
                var proactiveServiceTypes = ConfigurationManager.AppSettings["ProActiveTypes"];
                if (String.IsNullOrEmpty(proactiveServiceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "ProActiveTypes"));
                return proactiveServiceTypes.Split(';');
            }
        }

        public static string[] StandardWarrantyTypes
        {
            get
            {
                var stwServiceTypes = ConfigurationManager.AppSettings["StandardWarrantyTypes"];
                if (String.IsNullOrEmpty(stwServiceTypes))
                    throw new ConfigurationErrorsException(String.Format(ImportConstantMessages.CONFIGURATION_ERROR, "StandardWarrantyTypes"));
                return stwServiceTypes.Split(';');
            }
        }

        public static string[] AllServiceTypes
        {
            get
            {
                return AllowedServiceTypes.Union(ProActiveServices).Union(StandardWarrantyTypes).ToArray();
            }
        }
    }
}
