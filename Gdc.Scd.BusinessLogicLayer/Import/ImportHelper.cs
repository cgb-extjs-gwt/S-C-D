using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Import
{
    public static class ImportHelper
    {
        public static bool IsSoftware(string serviceType, IEnumerable<string> softwareTypes)
        {
            var result = false;

            if (String.IsNullOrEmpty(serviceType))
                return false;
            var wgTypes = serviceType.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(t => t.Trim());
            foreach (var wgType in wgTypes)
            {
                if (softwareTypes.Contains(wgType, StringComparer.OrdinalIgnoreCase))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
