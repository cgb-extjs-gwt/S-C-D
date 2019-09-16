using Gdc.Scd.OperationResult;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Ebis.InstallBase
{
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        var installBaseService = new InstallBaseService();
    //        try
    //        {
    //            installBaseService.UploadInstallBaseInfo();
    //        }
    //        catch (Exception ex)
    //        {
    //            installBaseService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
    //            Fujitsu.GDC.ErrorNotification.Logger.Error(ImportConstants.UNEXPECTED_ERROR, ex, null, null);
    //        }
    //    }
    //}

    public class InstallBaseJob
    {
        public OperationResult<bool> Output()
        {
            var result = new OperationResult<bool>();
            var installBaseService = new InstallBaseService();
            try
            {
                installBaseService.UploadInstallBaseInfo();
                result = new OperationResult<bool>
                {
                    IsSuccess = true,
                    Result = true
                };
            }
            catch (Exception ex)
            {
                installBaseService.Logger.Log(LogLevel.Fatal, ex, ImportConstants.UNEXPECTED_ERROR);
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
            return "InstallBaseJob";
        }
    }
}
