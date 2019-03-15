using Gdc.Scd.OperationResult;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Logistics
{
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        try
    //        {
    //            LogisticsImportService.UploadLogisticInfo();
    //        }
    //        catch (FileNotFoundException ex)
    //        {
    //            LogisticsImportService.Logger.Log(LogLevel.Info, ex.Message);
    //        }
    //        catch (Exception ex)
    //        {
    //            LogisticsImportService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
    //            Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
    //        }
    //    }
    //}
    public class LogisticsJob
    {
        public OperationResult<bool> Output()
        {
            var result = new OperationResult<bool>();
            var logisticsImportService = new LogisticsImportService();
    
            try
            {
                logisticsImportService.UploadLogisticInfo();
                result = new OperationResult<bool>
                {
                    IsSuccess = true,
                    Result = true
                };
            }
            catch (FileNotFoundException ex)
            {
                logisticsImportService.Logger.Log(LogLevel.Info, ex.Message);
            }
            catch (Exception ex)
            {
                logisticsImportService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
                Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
                result = new OperationResult<bool>
                {
                    IsSuccess = false,
                    Result = true
                };
            }
            return result;
        }
        /// <summary>
        /// Method should return job name
        /// which should be similar as "JobName" column in [JobsSchedule] table
        /// </summary>
        /// <returns>Job name</returns>
        public string WhoAmI()
        {
            return "LogisticsImportJob";
        }
    }
}
