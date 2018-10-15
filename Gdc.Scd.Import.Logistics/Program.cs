using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Logistics
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LogisticsImportService.UploadLogisticInfo();
            }
            catch (FileNotFoundException ex)
            {
                LogisticsImportService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch (Exception ex)
            {
                LogisticsImportService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
