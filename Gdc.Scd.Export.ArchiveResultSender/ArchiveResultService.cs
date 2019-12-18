using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.ArchiveResultSender.Abstract;
using System;

namespace Gdc.Scd.Export.ArchiveResultSender
{
    public class ArchiveResultService
    {
        private ILogger log;

        private IArchiveInfoGetter archive;

        private IEmailService email;

        public ArchiveResultService(
                IArchiveInfoGetter archive,
                IEmailService email,
                ILogger log
            )
        {
            this.archive = archive;
            this.email = email;
            this.log = log;
        }

        public virtual void Run()
        {
            var periodStart = DateTime.Now.AddDays(-6).Date;
            var periodEnd = DateTime.Now.Date.AddTicks(-1).AddDays(1);

            log.Info($"Generating archive result report for {periodStart.ToString(Config.DateFormat)} - {periodEnd.ToString(Config.DateFormat)}");
            log.Info($"Getting folder info from {Config.ScdFolder}");

            var info = archive.GetArchiveResults(periodStart, periodEnd);
            log.Info($"{info.Count} folders were received.");
            log.Info($"Sending report message to {Config.MailTo}");
            email.SendArchiveResultEmail(info, Config.MailTo, Config.MailFrom,
                periodStart.ToString(Config.DateFormat),
                periodEnd.ToString(Config.DateFormat));

            log.Info("Email was sent successfully");
        }
    }
}
