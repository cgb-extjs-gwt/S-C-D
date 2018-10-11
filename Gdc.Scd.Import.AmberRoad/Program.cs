using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            catch(FileNotFoundException ex)
            {
                AmberRoadImportService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch(Exception ex)
            {
                AmberRoadImportService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
