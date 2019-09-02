using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class WgNotificationInterceptor : IAfterAddingInterceptor<Wg>
    {
        private readonly IEmailService emailService;

        public WgNotificationInterceptor(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        public void Handle(Wg[] items)
        {
            emailService.SendNewWgEmail(items);
        }
    }
}
