using System;
using System.Collections.Generic;
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
        private readonly DomainMeta domainMeta;

        private readonly DomainEnitiesMeta domainEnitiesMeta;

        private readonly ISqlRepository sqlRepository;

        private readonly SmtpClient client;

        private readonly IUserRepository userRepository;

        public EmailService()
        {
            this.client = new SmtpClient
            {
                Port = 25,
                Host = "imrpool.fs.fujitsu.com",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
        }

        public EmailService(
            DomainMeta domainMeta, 
            DomainEnitiesMeta domainEnitiesMeta, 
            ISqlRepository sqlRepository, 
            IUserRepository userRepository)
         : this()
        {
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.sqlRepository = sqlRepository;
            this.userRepository = userRepository;
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

        public void SendNewWgEmail(IEnumerable<Wg> wgs)
        {
            var body = new StringBuilder("New warranty groups were added:");
            body.AppendLine();

            body.AppendLine("<ul>");

            foreach (var wg in wgs)
            {
                body.AppendLine($"<li>{wg.Name}</li>");
            }

            body.AppendLine("</ul>");

            var adminEmails = this.userRepository.GetAdmins().Select(admin => admin.Email).ToArray();

            this.Send("New warranty groups", body.ToString(), adminEmails);
        }

        private void Send(
            string subject, 
            string htmlBody, 
            IEnumerable<string> toAddresses, 
            string fromAddress = "SCD_Admin@ts.fujitsu.com")
        {
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

            this.client.Send(mailMessage);
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
