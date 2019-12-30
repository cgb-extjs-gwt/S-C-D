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

        void SendNewWgEmail(IEnumerable<Wg> wgs, IEnumerable<string> emails);

        void SendPortfolioNotifications(Wg[] wgs, User[] users, string portfolioUrl);
    }
}
