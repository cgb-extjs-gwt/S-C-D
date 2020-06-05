using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Gdc.Scd.Spooler
{
    public static class Config
    {
        public static List<string> RunOnlyJobs { get; } = new List<string>();

        static Config()
        {
            if (ConfigurationManager.AppSettings["IncludedOnlyJobs"] != null)
                RunOnlyJobs.AddRange(ConfigurationManager.AppSettings["IncludedOnlyJobs"]
                    .Split(';', ',')
                    .Select(job => job.Trim()));
        }
    }
}
