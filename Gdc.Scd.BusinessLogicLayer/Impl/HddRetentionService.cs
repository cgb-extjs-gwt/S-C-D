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
        private readonly IRepositorySet repositorySet;

        private readonly IWgRepository wgRepo;

        private readonly IRepository<HddRetentionView> hddViewRepo;

        private readonly IRepository<HddRetentionManualCost> hddRepo;

        public HddRetentionService(
                IRepositorySet repositorySet,
                IRepository<HddRetentionView> hddViewRepo,
                IRepository<HddRetentionManualCost> hddRepo,
                IWgRepository wgRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hddViewRepo = hddViewRepo;
            this.hddRepo = hddRepo;
            this.wgRepo = wgRepo;
        }

        public bool CanEdit(User usr)
        {
            return usr.IsAdmin;
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

            if (usr.IsAdmin)
            {
                queryDto = query.Select(x => new HddRetentionDto
                {
                    WgId = x.WgId,
                    Wg = x.Wg,
                    HddRetention = approved ? x.HddRet_Approved : x.HddRet, //show hdd retention calc value for admin only
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

        public void SaveCost(User changeUser, HddRetentionDto[] records)
        {
            if (!CanEdit(changeUser))
            {
                throw new System.ArgumentException("Illegal access. User does not have <scd admin> role");
            }

            var recordsId = records.Select(x => x.WgId);

            var entities = (from wg in wgRepo.GetAll().Where(x => recordsId.Contains(x.Id))
                            from hdd in hddRepo.GetAll().Where(x => x.Id == wg.Id).DefaultIfEmpty()
                            select new
                            {
                                Wg = wg,
                                Manual = hdd
                            })
                           .ToDictionary(x => x.Wg.Id, y => y);

            if (entities.Count == 0)
            {
                return;
            }

            ITransaction transaction = null;
            try
            {
                transaction = repositorySet.GetTransaction();

                foreach (var rec in records)
                {
                    if (!entities.ContainsKey(rec.WgId))
                    {
                        continue;
                    }

                    var e = entities[rec.WgId];
                    var hdd = e.Manual ?? new HddRetentionManualCost { Wg = e.Wg }; //create new if does not exist

                    hdd.TransferPrice = rec.TransferPrice;
                    hdd.ListPrice = rec.ListPrice;
                    hdd.DealerDiscount = rec.DealerDiscount;
                    hdd.ChangeUser = changeUser;
                    //
                    hddRepo.Save(hdd);
                }

                repositorySet.Sync();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
