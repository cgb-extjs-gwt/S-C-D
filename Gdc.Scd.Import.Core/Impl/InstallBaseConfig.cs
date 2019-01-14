using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Gdc.Scd.Import.Core.Impl
{
    public static class InstallBaseConfig
    {
        public static string CentralEuropeRegion { get; set; }
        public static Dictionary<string, string> CountryMatch { get; set; }

        static InstallBaseConfig()
        {
            CentralEuropeRegion = ConfigurationManager.AppSettings["CentralEuropeRegion"] ?? "Central Europe";
            CountryMatch = new Dictionary<string, string>();
            string countryMatchConfig = ConfigurationManager.AppSettings["CountryMatch"]; 
            if (!String.IsNullOrEmpty(countryMatchConfig))
            {
                var countryMatchEntries = countryMatchConfig.Split(';');
                foreach (var entry in countryMatchEntries)
                {
                    var match = entry.Split('-');
                    if (match.Length > 1)
                        CountryMatch.Add(match[0].Trim(), match[1].Trim());
                }
            }
        }
    }
}
