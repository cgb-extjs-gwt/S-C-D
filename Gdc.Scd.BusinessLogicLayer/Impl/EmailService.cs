using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
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
        public EmailService(DomainMeta domainMeta, DomainEnitiesMeta domainEnitiesMeta, ISqlRepository sqlRepository)
        {
            this.domainMeta = domainMeta;
            this.domainEnitiesMeta = domainEnitiesMeta;
            this.sqlRepository = sqlRepository;
            Client = new SmtpClient
            {
                Port = 25,
                Host = "mail.fsc.net",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
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
                $"<b>Hello {username},</br>Your values are not approved.</br>Reason for rejection:</b> {rejectionText}. <b>Approver’s name: </b> {approverName}";
        }

        private async Task<string> GetCountryNameAsync(string levelName, long regionId)
        {
            var entityMeta = domainEnitiesMeta.GetInputLevel(levelName);
            var ids = new List<long> {regionId};
            var regions = await sqlRepository.GetNameIdItems(entityMeta, entityMeta.IdField.Name, entityMeta.NameField.Name, ids.AsEnumerable());
            return regions.FirstOrDefault().Name;
        }
    }
}
