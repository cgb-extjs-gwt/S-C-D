using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class EmailService : IEmailService
    {
        private readonly string SmtpHost;

        private readonly string EmailSenderAddress;

        private readonly string WebApplicationHost;

        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ISqlRepository sqlRepository;

        private readonly SmtpClient client;

        public EmailService()
        {
            this.SmtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "imrpool.fs.fujitsu.com";
            this.EmailSenderAddress = ConfigurationManager.AppSettings["EmailSenderAddress"] ?? "SCD_Admin@ts.fujitsu.com";
            this.WebApplicationHost = ConfigurationManager.AppSettings["WebApplicationHost"] ?? "intranet.g02.fujitsu.local/scd";

            this.client = new SmtpClient
            {
                Port = 25,
                Host = this.SmtpHost,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
        }

        public EmailService(
            DomainMeta domainMeta, 
            DomainEnitiesMeta domainEnitiesMeta, 
            ISqlRepository sqlRepository)
         : this()
        {
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.sqlRepository = sqlRepository;
        }

        public async Task SendApprovalMailAsync(CostBlockHistory history)
        {
            var body = string.Concat(
                this.GenerateApprovedGreeting(history.EditUser.Name), 
                await this.GenerateDetailedBodyAsync(history));

            this.Send("Approval granted", body, new[] { history.EditUser.Email });
        }

        public async Task SendRejectedMailAsync(CostBlockHistory history, string rejectionText, string approverName)
        {
            var body = string.Concat(
                this.GenerateRejectGreeting(history.EditUser.Name, rejectionText, approverName), 
                await this.GenerateDetailedBodyAsync(history));

            this.Send("Approval was not granted", body, new[] { history.EditUser.Email });
        }

        public void SendArchiveResultEmail(IList<ArchiveFolderDto> archiveFolderData,
            string emailTo,
            string emailFrom,
            string periodStart, string periodEnd)
        {
            var body = this.GenerateArchiveEmailResult(archiveFolderData, periodStart, periodEnd);
            var toAddresses =
                emailTo.Split(";,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                       .Select(address => address.Trim())
                       .ToArray();

            this.Send($"Result of Archiving {periodStart}/{periodEnd}", body, toAddresses, emailFrom);
        }

        public void SendNewWgEmail(Wg[] wgs, User[] admins, User[] prsPsms)
        {
            const string Subject = "New warranty groups";

            var messageBuilder = BuildMessageBuilder();
            var adminMessage = messageBuilder.ToString();

            this.Send(Subject, adminMessage, admins);

            var tableViewUrl = $"{this.WebApplicationHost}/table-view";
            var link = BuildLink(tableViewUrl, "Centrla Data Input", new Dictionary<string, IEnumerable>
            {
                ["wg"] = wgs.Select(wg => wg.Name)
            });

            var prsPsmMessage = messageBuilder.AppendLine(link).ToString();

            this.Send(Subject, prsPsmMessage, prsPsms);

            StringBuilder BuildMessageBuilder()
            {
                var stringBuilder = new StringBuilder("New warranty groups were added:");
                stringBuilder.AppendLine();

                stringBuilder.AppendLine("<ul>");

                foreach (var wg in wgs)
                {
                    stringBuilder.AppendLine($"<li>{wg.Name}</li>");
                }

                stringBuilder.AppendLine("</ul>");

                return stringBuilder;
            }
        }

        public void SendPortfolioNotifications(Wg[] wgs, User[] toUsers, User[] bccUsers)
        {
            var wgIds = wgs.Select(wg => wg.Id).ToArray();
            var wgNames = string.Concat(wgs.Select(wg => $"<li>{wg.Name}</li>"));
            var portfolioUrl = $"{this.WebApplicationHost}/portfolio";
            var link = BuildLink(portfolioUrl, "Link on Portfolio", new Dictionary<string, IEnumerable>
            {
                ["wg"] = wgIds
            });

            var message = string.Concat(
                $"{RoleConstants.ScdAdmin} finished editing the portfolio by warranty groups:",
                $"<ul>{wgNames}</ul>",
                link);

            this.Send("Portfolio notification", message, toUsers, null, bccUsers);
        }

        private string BuildLink(string url, string text, IDictionary<string, IEnumerable> queryParams = null)
        {
            if (queryParams != null)
            {
                var urlPrams = 
                    queryParams.Select(keyValue => new
                               {
                                 keyValue.Key,
                                 Value = keyValue.Value.OfType<object>().ToArray()
                               })
                               .Where(keyValue => keyValue.Value.Length > 0)
                               .SelectMany(keyValue => keyValue.Value.Select(value => $"{keyValue.Key}={value}"));


                url = $"{url}?{string.Join("&", urlPrams)}";
            }

            return $"<a href=\"{url}\">{text}</a>";
        }

        private void Send(
            string subject, 
            string htmlBody, 
            IEnumerable<string> toAddresses, 
            string fromAddress = null,
            IEnumerable<string> bccAddresses = null)
        {
            if (fromAddress == null)
            {
                fromAddress = this.EmailSenderAddress;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                IsBodyHtml = true,
                Subject = $"SCD: {subject}",
                Body = htmlBody
            };

            foreach (var address in toAddresses)
            {
                mailMessage.To.Add(address);
            }

            if (bccAddresses != null)
            {
                foreach (var address in bccAddresses)
                {
                    mailMessage.Bcc.Add(address);
                }
            }

            this.client.Send(mailMessage);
        }

        private void Send(
            string subject,
            string htmlBody,
            IEnumerable<User> targetUsers,
            string fromAddress = null,
            IEnumerable<User> bccUsers = null)
        {
            var toEmails = GetEmails(targetUsers);
            var bccEmails = GetEmails(bccUsers);

            this.Send(subject, htmlBody, toEmails, fromAddress, bccEmails);

            IEnumerable<string> GetEmails(IEnumerable<User> users) => users.Select(user => user.Email);
        }

        private async Task<string> GenerateDetailedBodyAsync(CostBlockHistory history)
        {
            var costBlock = domainMeta.CostBlocks[history.Context.CostBlockId];
            var costElement = costBlock.CostElements[history.Context.CostElementId];

            var infos = new List<string>
            {
                $"</br><b>Cost Block: {costBlock.Name}</br>",
                $"Cost Element: {costElement.Name}</br>",
            };

            if (history.Context.RegionInputId.HasValue)
            {
                var countryName = await GetCountryNameAsync(costElement.RegionInput.Id, history.Context.RegionInputId.Value);

                infos.Add($"Country: {countryName}</br>");
            }

            infos.Add($"User name: {history.ApproveRejectUser.Name}</br>");
            infos.Add($"Date: {history.ApproveRejectDate.Value.Date.ToShortDateString()}</br></b>");

            return string.Concat(infos);
        }

        private string GenerateApprovedGreeting(string username)
        {
            return $"<b>Hello {username},</br>Your values have been approved.</b>";
        }

        private string GenerateRejectGreeting(string username, string rejectionText, string approverName)
        {
            return
                $"<b>Hello {username},</br>Your values are not approved.</br>Reason for rejection:</b> {rejectionText}.<br/><b>Approver’s name: </b> {approverName}";
        }

        private async Task<string> GetCountryNameAsync(string levelName, long regionId)
        {
            var entityMeta = domainEnitiesMeta.GetInputLevel(levelName);
            var ids = new List<long> {regionId};
            var regions = await sqlRepository.GetNameIdItems(entityMeta, entityMeta.IdField.Name, entityMeta.NameField.Name, ids.AsEnumerable());
            return regions.FirstOrDefault().Name;
        }

        private string GenerateArchiveEmailResult(IList<ArchiveFolderDto> archiveFolderData, string periodStart, string periodEnd)
        {
            var resultSb = new StringBuilder();
            resultSb = resultSb.Append($"<span style='{StyleConstants.BODY_STYLE}'>Hello team,<br/> Please find below summary archiving report for period <b>{periodStart}</b> - <b>{periodEnd}</b><span><br><br>");
            resultSb = resultSb.Append(
                $"<table cellpadding='5' style='{StyleConstants.TABLE_STYLE}'><thead><tr><th style='{StyleConstants.TABLE_TH_STYLE}'>Folder Name</th><th style='{StyleConstants.TABLE_TH_STYLE}'>Total File Count</th><th style='{StyleConstants.TABLE_TH_STYLE}'>Total File Size</th></tr></thead><tbody>");

            for (int rowIndex = 0; rowIndex < archiveFolderData.Count; rowIndex++)
            {
                resultSb.Append($"<tr style='{GetStyleForTableRow(rowIndex)}'><td style='{StyleConstants.TABLE_TD_STYLE}'>{archiveFolderData[rowIndex].Name}</td><td style='{StyleConstants.TABLE_TD_STYLE_CENTER}'>{archiveFolderData[rowIndex].FileCount}</td><td style='{StyleConstants.TABLE_TD_STYLE}'>{archiveFolderData[rowIndex].TotalFolderSize} MB</td></tr>");
            }

            resultSb.Append("</tbody></table><br/><br/>");
            resultSb.Append($"<span style='{StyleConstants.SIGN_STYLE}'>Kind Regards,<br /> SCD Team<span>");
            return resultSb.ToString();
        }

        private string GetStyleForTableRow(int rowIndex)
        {
            return rowIndex % 2 == 0 ? String.Empty : StyleConstants.TABLE_ODD_ROW_STYLE;
        }
    }
}
