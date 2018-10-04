using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.AmberRoad
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AmberRoadImportService.UploadTaxAndDuties();
            }
            catch(Exception ex)
            {
                AmberRoadImportService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
