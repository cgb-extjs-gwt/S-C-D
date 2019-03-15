using Gdc.Scd.OperationResult;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;

namespace Gdc.Scd.Spooler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Logging.Instance().logMessageLine("----------------------------------------------------------------------");
                Logging.Instance().logMessageLine(string.Format("Spooler started at {0} ", DateTime.Now.ToLongTimeString()));

                var dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + ConfigurationManager.AppSettings["JobPath"];
                Logging.Instance().logMessageLine(string.Format("Searching jobs in '{0}'", dllPath));

                var jobInstancesFiles = SearchForJobInstances(dllPath);
                Logging.Instance().logMessageLine(string.Format("Founded '{0}' in job's folder", jobInstancesFiles.Count()));

                var jobInstances = GetJobs(jobInstancesFiles);
                Logging.Instance().logMessageLine(string.Format("Founded '{0}' instances in job's folder", jobInstances.Count()));

                var activeJobInstances = GetActiveJobs(jobInstances);
                Logging.Instance().logMessageLine(string.Format("Active jobs for launch: {0}", activeJobInstances.Count));

                //var jobsForLaunch = CheckJobsForLaunch(activeJobInstances);

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

        static List<dynamic> GetActiveJobs(IEnumerable<dynamic> jobInstances)
        {
            var now = DateTime.Now;
            var activeJobs = new List<dynamic>();
            var dbContext = new Scd_2Entities();
            var shouldBeLaunch = new List<JobsSchedule>();

            foreach (var jobSchedule in dbContext.JobsSchedule.Where(js => js.Active).ToList())
            {
                //if (jobSchedule.TimeInHours - now.Hour != 1) continue;
                //if (jobSchedule.Daily ||
                //    jobSchedule.ExactDate.Date == now.Date ||
                //    jobSchedule.DayOfWeek == (int)now.DayOfWeek)
                //{
                //    shouldBeLaunch.Add(jobSchedule);
                //    continue;
                //}
                //var currentWeekNumber = GetCurrentWeekNumber(now, CultureInfo.CurrentCulture);
                //if (jobSchedule.MonthlyDayOfWeek != (int)now.DayOfWeek ||
                //    jobSchedule.MonthlyWeekNumber != currentWeekNumber) continue;
                shouldBeLaunch.Add(jobSchedule);
            }

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

        static IEnumerable<IOperationResult> LaunchJobs(IEnumerable<dynamic> activeJobInstances)
        {
            var executionResults = new List<OperationResult<bool>>();
            foreach (var activeJobInstance in activeJobInstances)
            {
                try
                {
                    var executionResult = activeJobInstance.Output();
                    executionResults.Add(new OperationResult<bool>
                    {
                        Result = executionResult.Result,
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
        static IEnumerable<string> SearchForJobInstances(string path)
        {
            var assemblyFileNames = new List<string>();
            try
            {
                assemblyFileNames.AddRange(Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return assemblyFileNames;
        }
        static IEnumerable<dynamic> GetJobs(IEnumerable<string> assemblyFileNames)
        {
            try
            {
                assemblyFileNames = assemblyFileNames.Where(x => x.Contains("Job.dll"));
                Logging.Instance().logMessageLine(string.Format("Assembly files count: '{0}'", assemblyFileNames.Count()));
                foreach (var assemblyFileName in assemblyFileNames)
                {
                    Logging.Instance().logMessageLine(string.Format("Assembly file name: '{0}'", assemblyFileName));
                }
                var result = (
                        from assemblyFileName in assemblyFileNames
                        select assemblyFileName
                        into absolutePath
                              where IsValidAssembly(absolutePath)
                              select Assembly.LoadFile(absolutePath)
                        into dll
                              from Type type in dll.GetExportedTypes()
                              where type.GetMethods().Any(x => x.Name.Contains("Output"))
                              select Activator.CreateInstance(type)

                    ).ToList();
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }
        static bool IsValidAssembly(string path)
        {
            try
            {
                Logging.Instance().logMessageLine(string.Format("Searching jobs in '{0}'", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + path));
                var assembly = Assembly.LoadFrom(path);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Instance().logMessageLine(string.Format("Error happened during loading assembly: '{0}'", ex.Message));
                return false;
            }
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
