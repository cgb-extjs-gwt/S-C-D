using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.ExchangeRates
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ExchangeRateService.UploadExchangeRates();
            }
            catch (FileNotFoundException ex)
            {
                ExchangeRateService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch (Exception ex)
            {
                ExchangeRateService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
            }
        }
    }
}
