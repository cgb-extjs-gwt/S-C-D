using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostBlockService : ICostBlockService
    {
        private readonly ICostBlockRepository costBlockRepository;

        private readonly IRepositorySet repositorySet;

        private readonly IUserService userService;

        private readonly ICostBlockFilterBuilder costBlockFilterBuilder;

        private readonly ISqlRepository sqlRepository;

        private readonly IQualityGateSevice qualityGateSevice;

        private readonly ICostBlockHistoryService costBlockHistoryService;

        private readonly DomainEnitiesMeta meta;

        public CostBlockService(
            ICostBlockRepository costBlockRepository, 
            IRepositorySet repositorySet,
            IUserService userService,
            ICostBlockFilterBuilder costBlockFilterBuilder,
            ISqlRepository sqlRepository,
            IQualityGateSevice qualityGateSevice,
            ICostBlockHistoryService costBlockHistoryService,
            DomainEnitiesMeta meta)
        {
            this.costBlockRepository = costBlockRepository;
            this.repositorySet = repositorySet;
            this.userService = userService;
            this.costBlockFilterBuilder = costBlockFilterBuilder;
            this.sqlRepository = sqlRepository;
            this.qualityGateSevice = qualityGateSevice;
            this.costBlockHistoryService = costBlockHistoryService;
            this.meta = meta;
        }

        public async Task<QualityGateResultSet> Update(EditInfo[] editInfos, ApprovalOption approvalOption, EditorType editorType)
        {
            var checkResult = new QualityGateResultSet();
            var editItemContexts = this.BuildEditItemContexts(editInfos).ToArray();

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
                        QualityGateResult = await this.qualityGateSevice.Check(editContext, editorType)
                    });
                }
            }

            if (!checkResult.HasErrors)
            {
                using (var transaction = this.repositorySet.GetTransaction())
                {
                    try
                    {
                        await this.costBlockRepository.Update(editInfos);

                        foreach (var editItemContext in editItemContexts)
                        {
                            await this.costBlockHistoryService.Save(editItemContext.Context, editItemContext.EditItems, approvalOption, editItemContext.Filter, editorType);
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

        public async Task UpdateByCoordinatesAsync(
            IEnumerable<CostBlockEntityMeta> costBlockMetas,
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    foreach (var costBlockMeta in costBlockMetas)
                    {
                        await this.costBlockRepository.UpdateByCoordinatesAsync(costBlockMeta, updateOptions);
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

        public void UpdateByCoordinates(
            IEnumerable<CostBlockEntityMeta> costBlockMetas, 
            IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            foreach (var costBlockMeta in costBlockMetas)
            {
                this.costBlockRepository.UpdateByCoordinates(costBlockMeta, updateOptions);
            }
        }

        public async Task UpdateByCoordinatesAsync(IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            await this.UpdateByCoordinatesAsync(this.meta.CostBlocks, updateOptions);
        }

        public void UpdateByCoordinates(IEnumerable<UpdateQueryOption> updateOptions = null)
        {
            this.UpdateByCoordinates(this.meta.CostBlocks, updateOptions);
        }

        public async Task<IEnumerable<NamedId>> GetCoordinateItems(HistoryContext context, string coordinateId)
        {
            var meta = this.meta.GetCostBlockEntityMeta(context);
            var referenceField = 
                meta.GetDomainCoordinateFields(context.CostElementId)
                    .First(field => field.Name == coordinateId);

            var userCountries = this.userService.GetCurrentUserCountries();
            var costBlockFilter = this.costBlockFilterBuilder.BuildRegionFilter(context, userCountries).Convert();
            var referenceFilter = this.costBlockFilterBuilder.BuildCoordinateItemsFilter(referenceField.ReferenceMeta);
            var notDeletedCondition = CostBlockQueryHelper.BuildNotDeletedCondition(meta, meta.Name);

            return 
                await this.sqlRepository.GetDistinctItems(
                    meta, 
                    referenceField.Name, 
                    costBlockFilter, 
                    referenceFilter,
                    notDeletedCondition);
        }

        public async Task<IEnumerable<NamedId>> GetDependencyItems(HistoryContext context)
        {
            IEnumerable<NamedId> filterItems = null;

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.DomainMeta.CostElements[context.CostElementId];

            if (costElementMeta.Dependency != null)
            {
                filterItems = await this.GetCoordinateItems(context, costElementMeta.Dependency.Id);
            }

            return filterItems;
        }

        public async Task<IEnumerable<NamedId>> GetRegions(HistoryContext context)
        {
            IEnumerable<NamedId> regions = null;

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.DomainMeta.CostElements[context.CostElementId];
            var userCountries = this.userService.GetCurrentUserCountries().ToArray();

            if (costElementMeta.RegionInput != null)
            {
                if (userCountries.Length == 0)
                {
                    regions = await this.sqlRepository.GetDistinctItems(context.CostBlockId, context.ApplicationId, costElementMeta.RegionInput.Id);
                }
                else
                {
                    regions = userCountries.Select(country => new NamedId { Id = country.Id, Name = country.Name }).ToArray();
                }
            }

            return regions;
        }

        public async Task<CostElementData> GetCostElementData(HistoryContext context)
        {
            return new CostElementData
            {
                DependencyItems = await this.GetDependencyItems(context),
                Regions = await this.GetRegions(context)
            };
        }

        private IEnumerable<EditItemContext> BuildEditItemContexts(IEnumerable<EditInfo> editInfos)
        {
            var filterCache = new Dictionary<string, IDictionary<string, long[]>>();

            foreach (var editInfo in editInfos)
            {
                var costElementGroups =
                    editInfo.ValueInfos.SelectMany(info => BuildCoordinateInfo(info.CoordinateFilter, editInfo.Meta).Select(coordinateInfo => new
                                       {
                                           CostElementValues = info.Values,
                                           CoordinateInfo = coordinateInfo
                                       }))
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

                            var filter = filterGroup.Key == null ? new Dictionary<string, long[]>() : filterGroup.Key;

                            var context = new HistoryContext
                            {
                                ApplicationId = editInfo.Meta.ApplicationId,
                                CostBlockId = editInfo.Meta.CostBlockId,
                                InputLevelId = inputLevelGroup.Key,
                                CostElementId = costElementGroup.Key
                            };

                            yield return new EditItemContext
                            {
                                Context = context,
                                EditItems = editItems,
                                Filter = filter
                            };
                        }
                    }
                }
            }

            IEnumerable<(IDictionary<string, long[]> Filter, (string Id, long Value) InputLevel)> BuildCoordinateInfo(
                IDictionary<string, long[]> coordinateFilter, 
                CostBlockEntityMeta meta)
            {
                var maxInputLevelMeta = meta.DomainMeta.GetMaxInputLevel(coordinateFilter.Keys);
                var filterKeys = coordinateFilter.Keys.Where(coordinateId => coordinateId != maxInputLevelMeta.Id).ToArray();
                var key = string.Join(
                    "_", 
                    filterKeys.Select(filterKey => $"{filterKey}_{string.Join("_", coordinateFilter[filterKey])}"));

                if (!filterCache.TryGetValue(key, out var filter))
                {
                    filter =
                        filterKeys.Select(filterKey => new KeyValuePair<string, long[]>(filterKey, coordinateFilter[filterKey]))
                                  .ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);

                    filterCache.Add(key, filter);
                }

                foreach (var inputLevelValue in coordinateFilter[maxInputLevelMeta.Id])
                {
                    yield return (filter, (maxInputLevelMeta.Id, inputLevelValue));
                }
            }
        }
    }
}
