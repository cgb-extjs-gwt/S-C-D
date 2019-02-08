using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class HddRetentionService : IHddRetentionService
    {
        private readonly IRepository<HddRetentionView> hddViewRepo;
        private readonly IRepository<HddRetentionManualCost> hddRepo;

        public HddRetentionService(
                IRepository<HddRetentionView> hddViewRepo,
                IRepository<HddRetentionManualCost> hddRepo
            )
        {
            this.hddViewRepo = hddViewRepo;
            this.hddRepo = hddRepo;
        }

        public async Task<(HddRetentionDto[] items, int total)> GetCost(
                User usr,
                bool approved,
                HddFilterDto filter,
                int start,
                int limit
            )
        {
            var query = hddViewRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Wg.HasValue, x => x.WgId == filter.Wg.Value);
            }

            var count = await query.GetCountAsync();

            IQueryable<HddRetentionDto> queryDto;

            if (usr.IsGlobal)
            {
                queryDto = query.Select(x => new HddRetentionDto
                {
                    WgId = x.WgId,
                    Wg = x.Wg,
                    HddRetention = approved ? x.HddRet_Approved : x.HddRet, //show for admin only
                    TransferPrice = x.TransferPrice,
                    ListPrice = x.ListPrice,
                    DealerDiscount = x.DealerDiscount,
                    DealerPrice = x.DealerPrice,
                    ChangeUserName = x.ChangeUserName,
                    ChangeUserEmail = x.ChangeUserEmail
                });
            }
            else
            {
                queryDto = query.Select(x => new HddRetentionDto
                {
                    WgId = x.WgId,
                    Wg = x.Wg,
                    TransferPrice = x.TransferPrice,
                    ListPrice = x.ListPrice,
                    DealerDiscount = x.DealerDiscount,
                    DealerPrice = x.DealerPrice,
                    ChangeUserName = x.ChangeUserName,
                    ChangeUserEmail = x.ChangeUserEmail
                });
            }

            var result = await queryDto.PagingAsync(start, limit);

            return (result, count);
        }

        public void SaveCost(User usr, HddRetentionDto[] items)
        {
        }
    }
}
