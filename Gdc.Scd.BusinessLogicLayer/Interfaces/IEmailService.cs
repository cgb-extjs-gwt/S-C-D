using Gdc.Scd.Core.Entities;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IEmailService
    {
        Task SendApprovalMailAsync(CostBlockHistory history);
        Task SendRejectedMailAsync(CostBlockHistory history, string rejectionText, string approverName);
    }
}
