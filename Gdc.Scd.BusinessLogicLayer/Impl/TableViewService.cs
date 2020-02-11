using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class TableViewService : ITableViewService
    {
        private readonly ITableViewRepository tableViewRepository;

        private readonly IUserService userService;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockHistoryService costBlockHistoryService;

        private readonly IQualityGateSevice qualityGateSevice;

        private readonly IDomainService<Wg> wgService;

        private readonly IRoleCodeService roleCodeService;

        private readonly ICostBlockService costBlockService;

        private readonly DomainEnitiesMeta meta;

        public TableViewService(
            ITableViewRepository tableViewRepository, 
            IUserService userService, 
            IRepositorySet repositorySet,
            ICostBlockHistoryService costBlockHistoryService,
            IQualityGateSevice qualityGateSevice,
            IDomainService<Wg> wgService,
            IRoleCodeService roleCodeService,
            ICostBlockService costBlockService,
            DomainEnitiesMeta meta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userService = userService;
            this.repositorySet = repositorySet;
            this.costBlockHistoryService = costBlockHistoryService;
            this.qualityGateSevice = qualityGateSevice;
            this.wgService = wgService;
            this.roleCodeService = roleCodeService;
            this.costBlockService = costBlockService;
            this.meta = meta;
        }

        public async Task<IEnumerable<Record>> GetRecords()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var records = await this.tableViewRepository.GetRecords(costBlockInfos);
            var recordDictionary = this.GetRecordDictionary(records);
            var wgs = this.GetWgs(recordDictionary.Keys);

            foreach (var wg in wgs)
            {
                var record = recordDictionary[wg.Id];

                record.WgRoleCodeId = wg.RoleCodeId;
                record.WgResponsiblePerson = wg.ResponsiblePerson;
                record.WgPsmRelease = wg.PsmRelease;
            }

            return records;
        }

        public async Task<QualityGateResultSet> UpdateRecords(IEnumerable<Record> records, ApprovalOption approvalOption)
        {
            QualityGateResultSet qualityGateResult;

            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var editInfos = this.tableViewRepository.BuildEditInfos(costBlockInfos, records).ToArray();

            qualityGateResult = editInfos.Length == 0
                ? new QualityGateResultSet()
                : await this.costBlockService.Update(editInfos, approvalOption, EditorType.TableView);

            var recordDictionary = this.GetRecordDictionary(records);
            var wgs = this.GetWgs(recordDictionary.Keys);
            var updatedWgs = new List<Wg>();

            foreach (var wg in wgs)
            {
                var record = recordDictionary[wg.Id];

                if (wg.RoleCodeId != record.WgRoleCodeId ||
                    wg.ResponsiblePerson != record.WgResponsiblePerson ||
                    wg.PsmRelease != record.WgPsmRelease)
                {
                    wg.RoleCodeId = record.WgRoleCodeId;
                    wg.ResponsiblePerson = record.WgResponsiblePerson;
                    wg.PsmRelease = record.WgPsmRelease;

                    updatedWgs.Add(wg);
                }
            }

            this.wgService.SaveWithoutTransaction(updatedWgs);

            return qualityGateResult;
        }

        public async Task<TableViewInfo> GetTableViewInfo()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();

            return new TableViewInfo
            {
                RecordInfo = await this.tableViewRepository.GetRecordInfo(costBlockInfos),
                CostBlockReferences = await this.tableViewRepository.GetReferences(costBlockInfos),
                DependencyItems = await this.tableViewRepository.GetDependencyItems(costBlockInfos),
                RoleCodeReferences = 
                    (await this.roleCodeService.GetAllActive())
                                        .Select(roleCode => new NamedId { Id = roleCode.Id, Name = roleCode.Name })
                                        .ToArray()
            };
        }

        public async Task<DataInfo<HistoryItemDto>> GetHistory(CostElementIdentifier costElementId, IDictionary<string, long> coordinates, QueryInfo queryInfo = null)
        {
            var historyContext = new CostElementContext
            {
                ApplicationId = costElementId.ApplicationId,
                CostBlockId = costElementId.CostBlockId,
                CostElementId = costElementId.CostElementId
            };

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(historyContext);
            var inputLevel = costBlockMeta.SliceDomainMeta.GetMaxInputLevel(coordinates.Keys);

            historyContext.InputLevelId = inputLevel.Id;

            var filter = coordinates.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

            return await this.costBlockHistoryService.GetHistory(historyContext, filter, queryInfo);
        }

        private IDictionary<long, Record> GetRecordDictionary(IEnumerable<Record> records)
        {
            return records.ToDictionary(record => record.Coordinates[MetaConstants.WgInputLevelName].Id);
        }

        private Wg[] GetWgs(IEnumerable<long> wgIds)
        {
            return this.wgService.GetAll().Where(wg => wgIds.Contains(wg.Id)).ToArray();
        }

        private IEnumerable<CostElementInfo> GetCostBlockInfo()
        {
            var user = this.userService.GetCurrentUser();

            foreach (var costBlock in this.meta.CostBlocks)
            {
                var fieldNames =
                    (from costElement in costBlock.SliceDomainMeta.CostElements
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

        private (string Id, long Value)? GetMaxInputLevel(
            IDictionary<string, long> coordinates,
            IDictionary<string, InputLevelMeta> inputLevelMetas)
        {
            InputLevelMeta maxInputLevelMeta = null;
            (string, long)? maxInputLevel = null;

            foreach (var coordinate in coordinates)
            {
                if (inputLevelMetas.TryGetValue(coordinate.Key, out var inputLevelMeta) &&
                    (maxInputLevelMeta == null || maxInputLevelMeta.LevelNumber < inputLevelMeta.LevelNumber))
                {
                    maxInputLevelMeta = inputLevelMeta;
                    maxInputLevel = (coordinate.Key, coordinate.Value);
                }
            }

            return maxInputLevel;
        }
    }
}
