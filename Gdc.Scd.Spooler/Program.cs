using Gdc.Scd.OperationResult;
using OperationResult;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Gdc.Scd.Spooler
{
    class Program
    {
        static void Main(string[] args)
        {
            var dllPath = ConfigurationManager.AppSettings["JobPath"];

            var jobInstancesFiles = SearchForJobInstances(dllPath);
            var jobInstances = GetJobs(jobInstancesFiles);

            var activeJobInstances = GetActiveJobs(jobInstances);
            //var jobsForLaunch = CheckJobsForLaunch(activeJobInstances);
            var jobExecutionResults = LaunchJobs(activeJobInstances);

            Console.ReadKey();
        }

        static List<dynamic> GetActiveJobs(IEnumerable<dynamic> jobInstances)
        {
            var now = DateTime.Now;
            var activeJobs = new List<dynamic>();
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
                if (jobSchedule.MonthlyDayOfWeek != (int) now.DayOfWeek ||
                    jobSchedule.MonthlyWeekNumber != currentWeekNumber) continue;
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

        //static IEnumerable<dynamic> CheckJobsForLaunch(List<dynamic> activeJobInstances)
        //{
        //    throw new NotImplementedException();
        //}

        static IEnumerable<IOperationResult> LaunchJobs(IEnumerable<dynamic> activeJobInstances)
        {
            var executionResults = new List<IOperationResult>();
            foreach (var activeJobInstance in activeJobInstances)
            {
                try
                {
                    IOperationResult executionResult = activeJobInstance.Output();
                    executionResults.Add(executionResult);
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
                assemblyFileNames.AddRange(Directory.EnumerateFiles(path, "*.dll*", SearchOption.AllDirectories));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return assemblyFileNames;
        }
        static IEnumerable<dynamic> GetJobs(IEnumerable<string> assemblyFileNames)
        {
            return (
                from assemblyFileName in assemblyFileNames
                select Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFileName)
                into absolutePath
                select Assembly.LoadFile(absolutePath)
                into dll
                from dynamic type in dll.GetExportedTypes()
                select Activator.CreateInstance(type)
                ).ToList();
        }
    }
}
