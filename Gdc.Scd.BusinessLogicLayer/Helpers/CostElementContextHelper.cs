using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class CostElementContextHelper
    {
        public static bool IsHardware(this CostElementContext c)
        {
            return string.Compare(c.ApplicationId, "Hardware", true) == 0;
        }

        public static bool IsSoftware(this CostElementContext c)
        {
            return string.Compare(c.ApplicationId, "SoftwareSolution", true) == 0;
        }
    }
}
