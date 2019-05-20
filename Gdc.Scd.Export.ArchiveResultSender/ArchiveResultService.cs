using Gdc.Scd.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Export.ArchiveResultSender.Abstract;
using Ninject;

namespace Gdc.Scd.Export.ArchiveResultSender
{
    public class ArchiveResultService
    {
        public ILogger<LogLevel> Logger { get; private set; }
        public IArchiveInfoGetter ArchiveInfoGetter { get; private set; }
        public IEmailService EmailService { get; private set; }

        public ArchiveResultService()
        {
            NinjectExt.IsConsoleApplication = true;
            IKernel kernel = CreateKernel();
            Logger = kernel.Get<ILogger<LogLevel>>();
            ArchiveInfoGetter = kernel.Get<IArchiveInfoGetter>();
            EmailService = kernel.Get<IEmailService>();
        }

        public void Process()
        {
            var periodStart = DateTime.Now.AddDays(-8).Date;
            var periodEnd = DateTime.Now.AddDays(-2).Date.AddTicks(-1).AddDays(1);
            
            Logger.Log(LogLevel.Info, $"Generating archive result report for {periodStart.ToString(Config.DateFormat)} - {periodEnd.ToString(Config.DateFormat)}");
            Logger.Log(LogLevel.Info, $"Getting folder info from {Config.ScdFolder}");

            var info = ArchiveInfoGetter.GetArchiveResults(periodStart, periodEnd);
            Logger.Log(LogLevel.Info, $"{info.Count} folders were received.");
            Logger.Log(LogLevel.Info, $"Sending report message to {Config.MailTo}");
            EmailService.SendArchiveResultEmail(info, Config.MailTo, periodStart.ToString(Config.DateFormat),
                periodEnd.ToString(Config.DateFormat));

            Logger.Log(LogLevel.Info, "Email was sent successfully");
        }

        private StandardKernel CreateKernel()
        {
            return new StandardKernel(new Module());
        }
    }
}
