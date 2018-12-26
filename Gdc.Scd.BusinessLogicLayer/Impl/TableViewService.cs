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
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class TableViewService : ITableViewService
    {
        private readonly ITableViewRepository tableViewRepository;

        private readonly IUserService userService;

        private readonly IRepositorySet repositorySet;

        private readonly ICostBlockHistoryService costBlockHistoryService;

        private readonly IQualityGateSevice qualityGateSevice;

        private readonly DomainEnitiesMeta meta;

        private readonly IDomainService<Wg> wgService;

        public TableViewService(
            ITableViewRepository tableViewRepository, 
            IUserService userService, 
            IRepositorySet repositorySet,
            ICostBlockHistoryService costBlockHistoryService,
            IQualityGateSevice qualityGateSevice,
            IDomainService<Wg> wgService,
            DomainEnitiesMeta meta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userService = userService;
            this.repositorySet = repositorySet;
            this.costBlockHistoryService = costBlockHistoryService;
            this.qualityGateSevice = qualityGateSevice;
            this.wgService = wgService;
            this.meta = meta;
        }

        public async Task<IEnumerable<Record>> GetRecords()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var records = await this.tableViewRepository.GetRecords(costBlockInfos);

            if (costBlockInfos.All(info => info.Meta.InputLevelFields[MetaConstants.WgInputLevelName] != null))
            {
                var wgIds = records.Select(record => record.Coordinates[MetaConstants.WgInputLevelName].Id).ToArray();
                var wgs = wgService.GetAll().Where(wg => wgIds.Contains(wg.Id)).Include(wg => wg.Pla).ToDictionary(wg => wg.Id);

                foreach (var record in records)
                {
                    if (record.Coordinates.TryGetValue(MetaConstants.WgInputLevelName, out var wgCoordinate) &&
                        wgs.TryGetValue(wgCoordinate.Id, out var wg))
                    {
                        record.AdditionalData.Add("Wg.PLA", wg.Pla.Name);
                        record.AdditionalData.Add("Wg.Description", wg.Description);
                        record.AdditionalData.Add("Wg.ResponsiblePerson", wg.ResponsiblePerson);
                    }
                }
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
                foreach(var editItemContext in editItemContexts)
                {
                    checkResult.Items.Add(new QualityGateResultSetItem
                    {
                        CostElementIdentifier = new CostElementIdentifier
                        {
                            ApplicationId = editItemContext.Context.ApplicationId,
                            CostBlockId = editItemContext.Context.CostBlockId,
                            CostElementId = editItemContext.Context.CostElementId,
                        },
                        QualityGateResult = await this.qualityGateSevice.Check(editItemContext.EditItems, editItemContext.Context, editItemContext.Filter, EditorType.TableView)
                    });
                }
            }

            if (!checkResult.HasErrors)
            {
                using (var transaction = this.repositorySet.GetTransaction())
                {
                    try
                    {
                        await this.tableViewRepository.UpdateRecords(editInfos);

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
                RecordInfo = this.tableViewRepository.GetRecordInfo(costBlockInfos),
                References = await this.tableViewRepository.GetReferences(costBlockInfos),
                DependencyItems = await this.tableViewRepository.GetDependencyItems(costBlockInfos)
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
            var coordinateInfo = this.BuildCoordinateInfo(coordinates, inputLevels);

            historyContext.InputLevelId = coordinateInfo.InputLevel.Id;

            var filter = coordinateInfo.Filter.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

            return await this.costBlockHistoryService.GetHistoryItems(historyContext, filter, queryInfo);
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
                                            CoordinateIfno = this.BuildCoordinateInfo(info.Coordinates, inputLevelMetas)
                                        })
                                       .SelectMany(info => info.CostElementValues.Select(costElemenValue => new
                                        {
                                            CostElementValue = costElemenValue,
                                            info.CoordinateIfno.Filter,
                                            info.CoordinateIfno.InputLevel
                                        }))
                                       .GroupBy(info => info.CostElementValue.Key);

                foreach (var costElementGroup in costElementGroups)
                {
                    foreach (var inputLevelGroup in costElementGroup.GroupBy(info => info.InputLevel.Id))
                    {
                        var context = new HistoryContext
                        {
                            ApplicationId = editInfo.Meta.ApplicationId,
                            CostBlockId = editInfo.Meta.CostBlockId,
                            InputLevelId = inputLevelGroup.Key,
                            CostElementId = costElementGroup.Key
                        };

                        var filterGroups = inputLevelGroup.GroupBy(info => info.Filter.Count == 0 ? null : info.Filter);

                        foreach (var filterGroup in filterGroups)
                        {
                            var editItems =
                                filterGroup.Select(info => new EditItem { Id = info.InputLevel.Value, Value = info.CostElementValue.Value })
                                           .ToArray();

                            var filter = filterGroup.Key == null
                                ? new Dictionary<string, long[]>()
                                : filterGroup.Key.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

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
            InputLevelMeta maxInputLevelMeta = null;
            (string, long)? inputLevel = null;

            foreach (var coordinate in coordinates)
            {
                var inputLevelMeta = inputLevelMetas[coordinate.Key];

                if (inputLevelMeta != null &&
                    (maxInputLevelMeta == null || maxInputLevelMeta.LevelNumber < inputLevelMeta.LevelNumber))
                {
                    maxInputLevelMeta = inputLevelMeta;
                    inputLevel = (coordinate.Key, coordinate.Value);
                }
            }

            var filter = new Dictionary<string, long>(coordinates);

            filter.Remove(maxInputLevelMeta.Id);

            return (filter, inputLevel.Value);
        }

        private IDictionary<string, InputLevelMeta> GetInputLevels(CostBlockEntityMeta meta)
        {
            return meta.DomainMeta.InputLevels.ToDictionary(inputLevel => inputLevel.Id);
        }
    }
}
