using System.Net.Mail;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class EmailService : IEmailService
    {
        public SmtpClient Client { get; set; }
        public EmailService()
        {
            Client = new SmtpClient
            {
                Port = 25,
                Host = "mail.fsc.net",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
        }

        public void SendApprovalMail(User recepient)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("SCD_Admin@ts.fujitsu.com"),
                IsBodyHtml = true,
                Subject = "SCD: Approval granted",
                Body = $"<b>Hello {recepient.Name},</br>Your values have been approved.</b>"
            };
            mailMessage.To.Add(recepient.Email);
            Client.Send(mailMessage);
        }
        public void SendRejectedMail(User recepient, string rejectionText, string approverName)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("SCD_Admin@ts.fujitsu.com"),
                Subject = "SCD: Approval was not granted",
                Body = $"<b>Hello {recepient.Name},</br>Your values are not approved.</br>Reason for rejection:</b> {rejectionText}. <b>Approver’s name: </b> {approverName}"
            };
            mailMessage.To.Add(recepient.Email);
            Client.Send(mailMessage);
        }
    }
}
