using System;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class DateHelper
    {
        public static string Timestamp()
        {
            var now = DateTime.Now;
            return Timestamp(ref now);
        }

        public static string Timestamp(ref DateTime d)
        {
            return d.ToString("yyyyMMddHHmmss");
        }
    }
}