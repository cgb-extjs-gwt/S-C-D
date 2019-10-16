using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public static class ImportHelper
    {
        private const string SOFTWARE = "Software";
        private const string SOFTWARE_TYPE = "SWP";
        private const string SOLUTION_TYPE = "SOL";
 
        public static bool IsSoftware(string serviceType, 
            string alignment)
        {
            if (String.IsNullOrEmpty(alignment))
                return false;

            if (String.IsNullOrEmpty(serviceType))
                return SOFTWARE.Equals(alignment, StringComparison.OrdinalIgnoreCase);


            return SOFTWARE.Equals(alignment, StringComparison.OrdinalIgnoreCase) &&
                   (serviceType.ToUpper().Contains(SOFTWARE_TYPE) ||
                    serviceType.ToUpper().Contains(SOLUTION_TYPE));
        }

        public static bool IsSolution(string serviceType, string solutionIdentifier)
        {
            if (String.IsNullOrEmpty(serviceType) || String.IsNullOrEmpty(solutionIdentifier))
                return false;

            return serviceType.ToLower().Contains(solutionIdentifier.ToLower());
        }
    }
}
