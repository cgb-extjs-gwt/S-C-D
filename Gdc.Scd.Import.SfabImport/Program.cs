using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.SfabImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SFabService.UploadSfabs();
            }
            catch (FileNotFoundException ex)
            {
                SFabService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch (Exception ex)
            {
                SFabService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
