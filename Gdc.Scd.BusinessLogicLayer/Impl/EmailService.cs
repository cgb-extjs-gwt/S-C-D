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
        public SmtpClient Client { get; set; }

        public EmailService()
        {
            Client = new SmtpClient
            {
                Port = 25,
                Host = "imrpool.fs.fujitsu.com",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
        }

        public EmailService(DomainMeta domainMeta, DomainEnitiesMeta domainEnitiesMeta, ISqlRepository sqlRepository)
         : this()
        {
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.sqlRepository = sqlRepository;
        }

        public async Task SendApprovalMailAsync(CostBlockHistory history)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("SCD_Admin@ts.fujitsu.com"),
                IsBodyHtml = true,
                Subject = "SCD: Approval granted",
            };
            var body = await GenerateDetailedBodyAsync(history);
            mailMessage.Body = string.Concat(GenerateApprovedGreeting(history.EditUser.Name), body);
            mailMessage.To.Add(history.EditUser.Email);
            Client.Send(mailMessage);
        }
        public async Task SendRejectedMailAsync(CostBlockHistory history, string rejectionText, string approverName)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("SCD_Admin@ts.fujitsu.com"),
                IsBodyHtml = true,
                Subject = "SCD: Approval was not granted",
            };
            var body = await GenerateDetailedBodyAsync(history);
            mailMessage.Body = string.Concat(GenerateRejectGreeting(history.EditUser.Name, rejectionText, approverName), body);
            mailMessage.To.Add(history.EditUser.Email);
            Client.Send(mailMessage);
        }

        public void SendArchiveResultEmail(IList<ArchiveFolderDto> archiveFolderData,
            string emailTo,
            string periodStart, string periodEnd)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailTo),
                IsBodyHtml = true,
                Subject = $"SCD: Result of Archiving {periodStart}/{periodEnd}"
            };

            var body = GenerateArchiveEmailResult(archiveFolderData, periodStart, periodEnd);
            mailMessage.Body = body;
            mailMessage.To.Add(emailTo);
            Client.Send(mailMessage);
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
