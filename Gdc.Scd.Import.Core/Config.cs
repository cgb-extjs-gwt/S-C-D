﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core
{
    public static class Config
    {
        public static string EmeiaRegionName { get; private set; }
        public static int ActualYear { get; private set; }

        static Config()
        {
            EmeiaRegionName = ConfigurationManager.AppSettings["Region"] ?? "EMEIA";
            int actualYear = 0;
            if (ConfigurationManager.AppSettings["Year"] != null && Int32.TryParse(ConfigurationManager.AppSettings["Year"], out actualYear))
                ActualYear = actualYear;
            else
                ActualYear = 5;
        }
    }
}