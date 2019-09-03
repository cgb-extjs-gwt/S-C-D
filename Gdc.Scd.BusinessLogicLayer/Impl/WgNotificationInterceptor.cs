using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class WgNotificationInterceptor : IAfterAddingInterceptor<Wg>
    {
        private readonly IEmailService emailService;

        private readonly INotifyChannel notifyChannel;

        private readonly IUserRepository userRepository;

        public WgNotificationInterceptor(IEmailService emailService, INotifyChannel notifyChannel, IUserRepository userRepository)
        {
            this.emailService = emailService;
            this.notifyChannel = notifyChannel;
            this.userRepository = userRepository;
        }

        public void Handle(Wg[] items)
        {
            var admins = this.userRepository.GetAdmins();

            this.emailService.SendNewWgEmail(items, admins.Select(admin => admin.Email));

            var message = $"New warranty groups were added: {string.Join(", ", items.Select(item => item.Name))}";

            foreach (var admin in admins)
            {
                this.notifyChannel.Send(admin.Login, message);
            }
        }
    }
}
