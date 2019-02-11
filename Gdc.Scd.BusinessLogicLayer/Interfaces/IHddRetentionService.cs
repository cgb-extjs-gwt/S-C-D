using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.Core.Entities;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IHddRetentionService
    {
        Task<(HddRetentionDto[] items, int total)> GetCost(
                User user,
                bool approved,
                HddFilterDto filter,
                int start,
                int limit
            );

        bool CanEdit(User usr);

        void SaveCost(User user, HddRetentionDto[] items);
    }
}
