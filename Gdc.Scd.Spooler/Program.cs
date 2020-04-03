using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Spooler.Core.Entities;
using Gdc.Scd.Spooler.Core.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Gdc.Scd.Spooler
{
    class Program
    {
        private static readonly StandardKernel kernel;
        
        static Program()
        {
            NinjectExt.IsConsoleApplication = true;

            kernel = CreateKernel();
        }

        static void Main(string[] args)
        {
            try
            {
                Logging.Instance().logMessageLine("----------------------------------------------------------------------");
                Logging.Instance().logMessageLine(string.Format("Spooler started at {0} ", DateTime.Now.ToLongTimeString()));
                var activeJobInstances = GetActiveJobs();
                Logging.Instance().logMessageLine(string.Format("Active jobs for launch: {0}", activeJobInstances.Count));

                var jobExecutionResults = LaunchJobs(activeJobInstances);
                Logging.Instance().logMessageLine(string.Format("Execution result: {0} instances returned 'success' message",
                    jobExecutionResults.Count(instance => instance.IsSuccess)));

                Logging.Instance().logMessageLine("----------------------------------------------------------------------");
                Logging.Instance().logMessageLine(string.Format("Spooler closing at {0} ", DateTime.Now.ToLongTimeString()));
            }
            catch (Exception ex)
            {
                Logging.Instance().logException(ex);
            }
        }

        static List<IJob> GetActiveJobs()
        {
            var now = DateTime.Now;
            var activeJobs = new List<IJob>();
            var dbContext = new Scd_2Entities();
            var shouldBeLaunch = new List<JobsSchedule>();

            foreach (var jobSchedule in dbContext.JobsSchedule.Where(js => js.Active).ToList())
            {
                if (jobSchedule.TimeInHours - now.Hour != 1) continue;
                if (jobSchedule.Daily ||
                    jobSchedule.ExactDate.Date == now.Date ||
                    jobSchedule.DayOfWeek == (int)now.DayOfWeek)
                {
                    shouldBeLaunch.Add(jobSchedule);
                    continue;
                }
                var currentWeekNumber = GetCurrentWeekNumber(now, CultureInfo.CurrentCulture);
                if (jobSchedule.MonthlyDayOfWeek != (int)now.DayOfWeek ||
                    jobSchedule.MonthlyWeekNumber != currentWeekNumber) continue;
                shouldBeLaunch.Add(jobSchedule);
            }

            var jobInstances = kernel.GetAll<IJob>().ToArray();

            foreach (var jobSchedule in shouldBeLaunch)
            {
                activeJobs.AddRange(
                    jobInstances
                        .Where(
                            jobInstance => jobSchedule.JobName.Contains(jobInstance.WhoAmI()
                            )));
            }

            return activeJobs;
        }

        static int GetCurrentWeekNumber(DateTime date, CultureInfo culture)
        {
            return culture.Calendar.GetWeekOfYear(date,
                culture.DateTimeFormat.CalendarWeekRule,
                culture.DateTimeFormat.FirstDayOfWeek);
        }

        static IEnumerable<IOperationResult> LaunchJobs(IEnumerable<IJob> activeJobInstances)
        {
            var executionResults = new List<OperationResult<bool>>();
            foreach (var activeJobInstance in activeJobInstances)
            {
                try
                {
                    var executionResult = activeJobInstance.Output();
                    executionResults.Add(new OperationResult<bool>
                    {
                        IsSuccess = executionResult.IsSuccess
                    });
                }
                catch (Exception e)
                {
                    executionResults.Add(
                        new OperationResult<bool>
                        {
                            IsSuccess = false,
                            ErrorMessage = e.ToString()
                        }
                    );
                }
            }
            return executionResults;
        }

        private static StandardKernel CreateKernel()
        {
            return new StandardKernel(new Module());
        }
    }
    public class Logging
    {
        private static Logging __logging = null;

        private StreamWriter _logger = null;
        private string _fileName = null;

        public static Logging Instance()
        {
            if (__logging == null)
                __logging = new Logging();
            return __logging;
        }

        private Logging()
        {

            string logFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string formattedDate = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString();
            logFilePath = logFilePath + "\\" + "Scd_Spooler_" + formattedDate + ".txt";

            _fileName = logFilePath;
        }

        public void logMessageLine(string line)
        {
            _logger = new StreamWriter(_fileName, true);

            _logger.WriteLine(line);

            _logger.Close();
        }

        public void logException(Exception exc)
        {
            _logger = new StreamWriter(_fileName, true);

            _logger.WriteLine("Exception occurred:");
            _logger.Write(exc.ToString());
            _logger.Write(Environment.NewLine);

            _logger.Close();
        }

        internal void LogStdOut()
        {
            this._logger = new StreamWriter(_fileName, true);
            Console.SetOut(this._logger);
        }

        internal void ResetStdOut()
        {
            this._logger.Close();
            StreamWriter stdOut = new StreamWriter(Console.OpenStandardOutput());
            stdOut.AutoFlush = true;
            Console.SetOut(stdOut);
        }
    }
}
