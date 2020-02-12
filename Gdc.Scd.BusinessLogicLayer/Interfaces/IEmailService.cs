using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using System.Threading.Tasks;
using Gdc.Scd.Core.Dto;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IEmailService
    {
        Task SendApprovalMailAsync(CostBlockHistory history);

        Task SendRejectedMailAsync(CostBlockHistory history, string rejectionText, string approverName);

        void SendArchiveResultEmail(IList<ArchiveFolderDto> archiveFolderData,
            string emailTo,
            string emailFrom,
            string periodStart, string periodEnd);

        void SendNewWgEmail(Wg[] wgs, User[] admins, User[] prsPsms);

        void SendPortfolioNotifications(Wg[] wgs, User[] toUsers, User[] bccUsers);
    }
}
