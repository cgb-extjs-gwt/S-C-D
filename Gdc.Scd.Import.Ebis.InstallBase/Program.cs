using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Ebis.InstallBase
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                InstallBaseService.UploadInstallBaseInfo();
            }
            catch (FileNotFoundException ex)
            {
                InstallBaseService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch (Exception ex)
            {
                InstallBaseService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
