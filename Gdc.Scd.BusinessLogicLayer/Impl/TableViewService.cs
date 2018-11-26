using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class TableViewService : ITableViewService
    {
        private readonly ITableViewRepository tableViewRepository;

        private readonly IUserService userService;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockHistoryService costBlockHistoryService;

        private readonly DomainEnitiesMeta meta;

        public TableViewService(
            ITableViewRepository tableViewRepository, 
            IUserService userService, 
            IRepositorySet repositorySet,
            ICostBlockHistoryService costBlockHistoryService,
            DomainEnitiesMeta meta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userService = userService;
            this.repositorySet = repositorySet;
            this.costBlockHistoryService = costBlockHistoryService;
            this.meta = meta;
        }

        public async Task<IEnumerable<Record>> GetRecords()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var records = await this.tableViewRepository.GetRecords(costBlockInfos);

            var wgs = repositorySet.GetRepository<Wg>().GetAll().Include(x => x.Pla);
            foreach(var record in records)
            {
                if (record.Coordinates.TryGetValue("Wg", out NamedId wgCoordinate))
                {
                    var wg = wgs.Where(x => x.Name == wgCoordinate.Name).FirstOrDefault();
                    if (wg != null)
                    {
                        record.AdditionalData.Add("Wg.PLA", wg.Pla.Name);
                        record.AdditionalData.Add("Wg.Description", wg.Description);
                    }
                }
            }

            return records;
        }

        public async Task UpdateRecords(IEnumerable<Record> records, bool isApproving)
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var editInfos = this.tableViewRepository.BuildEditInfos(costBlockInfos, records).ToArray();

            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    await this.tableViewRepository.UpdateRecords(editInfos);

                    await this.costBlockHistoryService.Save(editInfos, new ApprovalOption { IsApproving = isApproving });

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }
        }

        public async Task<TableViewInfo> GetTableViewInfo()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();

            return new TableViewInfo
            {
                RecordInfo = this.tableViewRepository.GetTableViewRecordInfo(costBlockInfos),
                References = await this.tableViewRepository.GetReferences(costBlockInfos)
            };
        }

        private IEnumerable<CostElementInfo> GetCostBlockInfo()
        {
            var user = this.userService.GetCurrentUser();

            foreach (var costBlock in this.meta.CostBlocks)
            {
                var fieldNames =
                    (from costElement in costBlock.DomainMeta.CostElements
                     where costElement.TableViewRoles != null && user.Roles.Any(role => costElement.TableViewRoles.Contains(role.Name))
                     select costElement.Id).ToArray();

                if (fieldNames.Length > 0)
                {
                    yield return new CostElementInfo
                    {
                        Meta = costBlock,
                        CostElementIds = fieldNames
                    };
                }
            }
        }
    }
}
