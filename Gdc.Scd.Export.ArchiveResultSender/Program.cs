using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Import.Core;
using Gdc.Scd.OperationResult;
using NLog;

namespace Gdc.Scd.Export.ArchiveResultSender
{
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        var archiveResultService = new ArchiveResultService();
    //        try
    //        {
    //            archiveResultService.Process();
    //        }
    //        catch (Exception ex)
    //        {
    //            archiveResultService.Logger.Log(LogLevel.Fatal, ex, "Unexpected error occured");
    //            Fujitsu.GDC.ErrorNotification.Logger.Error("Unexpected error occured", ex, null, null);
    //        }
    //    }
    //}

    public class ArchiveResultSenderJob
    {
        public OperationResult<bool> Output()
        {
            var result = new OperationResult<bool>();
            var archiveResultService = new ArchiveResultService();
            try
            {
                archiveResultService.Process();
                result = new OperationResult<bool>
                {
                    IsSuccess = true,
                    Result = true
                };
            }
            catch (Exception ex)
            {
                archiveResultService.Logger.Log(LogLevel.Fatal, ex, "Unexpected error occured");
                Fujitsu.GDC.ErrorNotification.Logger.Error("Unexpected error occured", ex, null, null);
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
            return "ArchiveResultSenderJob";
        }
    }
}
