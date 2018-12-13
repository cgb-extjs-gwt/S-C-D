using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IEmailService
    {
        void SendApprovalMail(User recepient);
        void SendRejectedMail(User recepient, string rejectionText, string approverName);
    }
}
