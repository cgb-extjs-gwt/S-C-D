using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.Core.Entities;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IHddRetentionService
    {
        Task<(HddRetentionDto[] items, bool hasMore)> GetCost(
                User user,
                bool approved,
                HddFilterDto filter,
                int start,
                int limit
            );

        bool CanEdit(User changeUser);

        void SaveCost(User changeUser, HddRetentionDto[] items);
    }
}
