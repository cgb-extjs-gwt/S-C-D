using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public static class ImportHelper
    {
        private const string SOFTWARE = "Software";

        public static bool IsSoftware(string serviceType, 
            IEnumerable<string> softwareTypes,
            string alignment)
        {
            var result = false;

            if (String.IsNullOrEmpty(serviceType))
                return SOFTWARE.Equals(alignment, StringComparison.OrdinalIgnoreCase);

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

        public static bool IsSolution(string serviceType, string solutionIdentifier)
        {
            if (String.IsNullOrEmpty(serviceType) || String.IsNullOrEmpty(solutionIdentifier))
                return false;

            return serviceType.ToLower().Contains(solutionIdentifier.ToLower());
        }
    }
}
