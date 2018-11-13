using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Ebis.Afr
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AfrService.UploadAfrInfo();
            }
            catch (FileNotFoundException ex)
            {
                AfrService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch (Exception ex)
            {
                AfrService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
