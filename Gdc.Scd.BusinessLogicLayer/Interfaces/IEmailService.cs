using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(User recepient, string subject, string message);
    }
}
