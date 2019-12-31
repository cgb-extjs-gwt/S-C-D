using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class WgNotificationInterceptor : IAfterAddingInterceptor<Wg>
    {
        private readonly IEmailService emailService;

        private readonly INotifyChannel notifyChannel;

        private readonly IUserService userService;

        public WgNotificationInterceptor(IEmailService emailService, INotifyChannel notifyChannel, IUserService userService)
        {
            this.emailService = emailService;
            this.notifyChannel = notifyChannel;
            this.userService = userService;
        }

        public void Handle(Wg[] items)
        {
            try
            {
                var admins = this.userService.GetAdmins().ToArray();
                var prsPsms = this.userService.GetPrsPsmUsers().ToArray();

                this.emailService.SendNewWgEmail(items, admins, prsPsms);

                var message = $"New warranty groups were added: {string.Join(", ", items.Select(item => item.Name))}";

                foreach (var admin in admins)
                {
                    this.notifyChannel.Send(admin.Login, message);
                }
            }
            catch
            {
                //TODO: need add logging
            }
        }
    }
}
