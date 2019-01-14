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

        static InstallBaseConfig()
        {
            CentralEuropeRegion = ConfigurationManager.AppSettings["CentralEuropeRegion"] ?? "Central Europe";
        }
    }
}
