using Gdc.Scd.Import.ExchangeRates;
using Gdc.Scd.OperationResult;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Spooler.JobLibraries.ExchangeRateJob
{
    public class ExchangeRateJob
    {
        public OperationResult<bool> Output()
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

            var result = new OperationResult<bool>
            {
                IsSuccess = true,
                Result = true
            };

            return result;
        }
        /// <summary>
        /// Method should return job name
        /// which should be similar as "JobName" column in [JobsSchedule] table
        /// </summary>
        /// <returns>Job name</returns>
        public string WhoAmI()
        {
            return "Exchange Rate Job";
        }
    }
}
