using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
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

        private readonly IDomainService<RoleCode> roleCodeService;

        private readonly DomainEnitiesMeta meta;

        public TableViewService(
            ITableViewRepository tableViewRepository, 
            IUserService userService, 
            IRepositorySet repositorySet,
            ICostBlockHistoryService costBlockHistoryService,
            IQualityGateSevice qualityGateSevice,
            IDomainService<Wg> wgService,
            IDomainService<RoleCode> roleCodeService,
            DomainEnitiesMeta meta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userService = userService;
            this.repositorySet = repositorySet;
            this.costBlockHistoryService = costBlockHistoryService;
            this.qualityGateSevice = qualityGateSevice;
            this.wgService = wgService;
            this.roleCodeService = roleCodeService;
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
            }

            return records;
        }

        public async Task<QualityGateResultSet> UpdateRecords(IEnumerable<Record> records, ApprovalOption approvalOption)
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var editInfos = this.tableViewRepository.BuildEditInfos(costBlockInfos, records).ToArray();
            var editItemContexts = this.BuildEditItemContexts(editInfos).ToArray();

            var checkResult = new QualityGateResultSet();

            if (approvalOption.IsApproving && !approvalOption.HasQualityGateErrors)
            {
                var editContextGroups = editItemContexts.GroupBy(editItemContext => new
                {
                    editItemContext.Context.ApplicationId,
                    editItemContext.Context.CostBlockId,
                    editItemContext.Context.CostElementId,
                    editItemContext.Context.InputLevelId,
                    editItemContext.Context.RegionInputId
                });

                foreach (var editContextGroup in editContextGroups)
                {
                    var editContext = new EditContext
                    {
                        Context = new HistoryContext
                        {
                            ApplicationId = editContextGroup.Key.ApplicationId,
                            CostBlockId = editContextGroup.Key.CostBlockId,
                            CostElementId = editContextGroup.Key.CostElementId,
                            InputLevelId = editContextGroup.Key.InputLevelId,
                            RegionInputId = editContextGroup.Key.RegionInputId
                        },
                        EditItemSets = editContextGroup.Select(editItemContex => new EditItemSet
                        {
                            EditItems = editItemContex.EditItems,
                            CoordinateFilter = editItemContex.Filter
                        }).ToArray()
                    };

                    checkResult.Items.Add(new QualityGateResultSetItem
                    {
                        CostElementIdentifier = new CostElementIdentifier
                        {
                            ApplicationId = editContextGroup.Key.ApplicationId,
                            CostBlockId = editContextGroup.Key.CostBlockId,
                            CostElementId = editContextGroup.Key.CostElementId,
                        },
                        QualityGateResult = await this.qualityGateSevice.Check(editContext, EditorType.TableView)
                    });
                }
            }

            if (!checkResult.HasErrors)
            {
                using (var transaction = this.repositorySet.GetTransaction())
                {
                    try
                    {
                        await this.UpdateRecords(records, editInfos);

                        foreach(var editItemContext in editItemContexts)
                        {
                            await this.costBlockHistoryService.Save(editItemContext.Context, editItemContext.EditItems, approvalOption, editItemContext.Filter, EditorType.TableView);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();

                        throw;
                    }
                }
            }

            return checkResult;
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
                    this.roleCodeService.GetAll()
                                        .Select(roleCode => new NamedId { Id = roleCode.Id, Name = roleCode.Name })
                                        .ToArray()
            };
        }

        public async Task<IEnumerable<HistoryItem>> GetHistoryItems(CostElementIdentifier costElementId, IDictionary<string, long> coordinates, QueryInfo queryInfo = null)
        {
            var historyContext = new HistoryContext
            {
                ApplicationId = costElementId.ApplicationId,
                CostBlockId = costElementId.CostBlockId,
                CostElementId = costElementId.CostElementId
            };

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(historyContext);
            var inputLevels = this.GetInputLevels(costBlockMeta);
            var inputLevel = this.GetMaxInputLevel(coordinates, inputLevels);

            historyContext.InputLevelId = inputLevel.Value.Id;

            var filter = coordinates.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

            return await this.costBlockHistoryService.GetHistoryItems(historyContext, filter, queryInfo);
        }

        private async Task UpdateRecords(IEnumerable<Record> records, EditInfo[] editInfos)
        {
            if (editInfos.Length > 0)
            {
                await this.tableViewRepository.UpdateRecords(editInfos);
            }

            var recordDictionary = this.GetRecordDictionary(records);
            var wgs = this.GetWgs(recordDictionary.Keys);
            var updatedWgs = new List<Wg>();

            foreach (var wg in wgs)
            {
                var record = recordDictionary[wg.Id];

                if (wg.RoleCodeId != record.WgRoleCodeId ||
                    wg.ResponsiblePerson != record.WgResponsiblePerson)
                {
                    wg.RoleCodeId = record.WgRoleCodeId;
                    wg.ResponsiblePerson = record.WgResponsiblePerson;

                    updatedWgs.Add(wg);
                }
            }

            this.wgService.SaveWithoutTransaction(updatedWgs);
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

        private IEnumerable<(HistoryContext Context, EditItem[] EditItems, Dictionary<string, long[]> Filter)> BuildEditItemContexts(IEnumerable<EditInfo> editInfos)
        {
            foreach (var editInfo in editInfos)
            {
                var inputLevelMetas = this.GetInputLevels(editInfo.Meta);

                var costElementGroups =
                    editInfo.ValueInfos.Select(info => new
                                        {
                                            CostElementValues = info.Values,
                                            CoordinateInfo = this.BuildCoordinateInfo(
                                                info.Filter.ToDictionary(
                                                    keyValue => keyValue.Key, 
                                                    keyValue => (long)keyValue.Value.First()), 
                                                inputLevelMetas)
                                        })
                                       .SelectMany(info => info.CostElementValues.Select(costElemenValue => new
                                        {
                                            CostElementValue = costElemenValue,
                                            info.CoordinateInfo.Filter,
                                            info.CoordinateInfo.InputLevel
                                        }))
                                       .GroupBy(info => info.CostElementValue.Key);

                foreach (var costElementGroup in costElementGroups)
                {
                    foreach (var inputLevelGroup in costElementGroup.GroupBy(info => info.InputLevel.Id))
                    {
                        var filterGroups = inputLevelGroup.GroupBy(info => info.Filter.Count == 0 ? null : info.Filter);

                        foreach (var filterGroup in filterGroups)
                        {
                            var editItems =
                                filterGroup.Select(info => new EditItem { Id = info.InputLevel.Value, Value = info.CostElementValue.Value })
                                           .ToArray();

                            var filter = filterGroup.Key == null
                                ? new Dictionary<string, long[]>()
                                : filterGroup.Key.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

                            var context = new HistoryContext
                            {
                                ApplicationId = editInfo.Meta.ApplicationId,
                                CostBlockId = editInfo.Meta.CostBlockId,
                                InputLevelId = inputLevelGroup.Key,
                                CostElementId = costElementGroup.Key
                            };

                            yield return (context, editItems, filter);
                        }
                    }
                }
            }
        }

        private (IDictionary<string, long> Filter, (string Id, long Value) InputLevel) BuildCoordinateInfo(
            IDictionary<string, long> coordinates,
            IDictionary<string, InputLevelMeta> inputLevelMetas)
        {
            var maxInputLevel = this.GetMaxInputLevel(coordinates, inputLevelMetas);
            var filter = new Dictionary<string, long>(coordinates);

            filter.Remove(maxInputLevel.Value.Id);

            return (filter, maxInputLevel.Value);
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

        private IDictionary<string, InputLevelMeta> GetInputLevels(CostBlockEntityMeta meta)
        {
            return meta.DomainMeta.InputLevels.ToDictionary(inputLevel => inputLevel.Id);
        }
    }
}
