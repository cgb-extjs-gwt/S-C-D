using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;

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
                var editContextGroups = editItemContexts.GroupBy(editItemContext => new CostElementContext
                {
                    ApplicationId = editItemContext.Context.ApplicationId,
                    CostBlockId = editItemContext.Context.CostBlockId,
                    CostElementId = editItemContext.Context.CostElementId,
                    InputLevelId = editItemContext.Context.InputLevelId,
                    RegionInputId = editItemContext.Context.RegionInputId
                });

                foreach (var editContextGroup in editContextGroups)
                {
                    var costBlock = this.meta.GetCostBlockEntityMeta(editContextGroup.Key);
                    var regionInputId = costBlock.DomainMeta.CostElements[editContextGroup.Key.CostElementId].RegionInput?.Id;

                    IEnumerable<EditItemSet> editItemSets = null;

                    if (regionInputId == null)
                    {
                        editItemSets = editContextGroup.Select(editItemContex => new EditItemSet
                        {
                            EditItems = editItemContex.EditItems,
                            CoordinateFilter = editItemContex.Filter
                        });
                    }
                    else
                    {
                        editItemSets = editContextGroup.Select(editItemContex => new EditItemSet
                        {
                            EditItems = editItemContex.EditItems,
                            CoordinateFilter =
                                 editItemContex.Filter.Where(keyValue => keyValue.Key != regionInputId)
                                                      .ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value)
                        });
                    }

                    var editContext = new EditContext
                    {
                        Context = new CostElementContext
                        {
                            ApplicationId = editContextGroup.Key.ApplicationId,
                            CostBlockId = editContextGroup.Key.CostBlockId,
                            CostElementId = editContextGroup.Key.CostElementId,
                            InputLevelId = editContextGroup.Key.InputLevelId,
                            RegionInputId = editContextGroup.Key.RegionInputId
                        },
                        EditItemSets = editItemSets.ToArray()
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
                await this.Update(editInfos, approvalOption, editorType, editItemContexts);
            }

            return checkResult;
        }

        public async Task<CostBlockHistory[]> UpdateWithoutQualityGate(
            EditInfo[] editInfos, 
            ApprovalOption approvalOption, 
            EditorType editorType,
            User currentUser = null)
        {
            var editItemContexts = this.BuildEditItemContexts(editInfos).ToArray();

            return await this.Update(editInfos, approvalOption, editorType, editItemContexts, currentUser);
        }

        public async Task UpdateAsApproved(EditInfo[] editInfos, EditorType editorType, User currentUser = null)
        {
            var editItemContexts = this.BuildEditItemContexts(editInfos).ToArray();
            var approvalOption = new ApprovalOption
            {
                TurnOffNotification = true,
            };

            var approvedEditInfos = 
                editInfos.Select(editInfo => new EditInfo
                         { 
                            Meta = editInfo.Meta,
                            ValueInfos = 
                                editInfo.ValueInfos.Select(valueInfo => BuildValuesInfoWithApprovedFields(editInfo.Meta, valueInfo))
                                                   .ToArray()
                         })
                         .ToArray();

            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    await this.costBlockRepository.Update(approvedEditInfos);

                    await this.costBlockHistoryService.SaveAsApproved(editItemContexts, approvalOption, editorType, currentUser);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }

            ValuesInfo BuildValuesInfoWithApprovedFields(CostBlockEntityMeta meta, ValuesInfo valuesInfo)
            {
                var apprvedValues =
                    valuesInfo.Values.Select(
                        keyValue =>
                            new KeyValuePair<string, object>(
                                meta.GetApprovedCostElement(keyValue.Key).Name,
                                keyValue.Value));

                return new ValuesInfo
                {
                    CoordinateFilter = valuesInfo.CoordinateFilter,
                    Values = valuesInfo.Values.Concat(apprvedValues).ToDictionary(x => x.Key, x => x.Value)
                };
            }
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

        public async Task<IEnumerable<NamedId>> GetCoordinateItems(CostElementContext context, string coordinateId)
        {
            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var referenceField = costBlockMeta.GetDomainCoordinateField(context.CostElementId, coordinateId);

            var userCountries = this.userService.GetCurrentUserCountries();
            var costBlockFilter = this.costBlockFilterBuilder.BuildRegionFilter(context, userCountries).Convert();
            var referenceFilter = this.costBlockFilterBuilder.BuildCoordinateItemsFilter(costBlockMeta, context.CostElementId, coordinateId);

            return
                await this.GetCoordinateItems(costBlockMeta, referenceField.Name, costBlockFilter, referenceFilter);
        }

        public async Task<IEnumerable<NamedId>> GetDependencyItems(CostElementContext context)
        {
            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.DomainMeta.CostElements[context.CostElementId];

            if (costElementMeta.Dependency == null)
            {
                return null; //no dependency, it's ok
            }

            if (context.IsHardware())
            {
                //Find items from Portfolio and Standard warranty
                return await new PortfolioInputService(repositorySet)
                               .GetCoordinateItemsByPorfolio(context.RegionInputId.Value, costElementMeta.Dependency.Id);
            }
            else
            {
                //simple get from config
                return await this.GetCoordinateItems(context, costElementMeta.Dependency.Id);
            }
        }

        public async Task<IEnumerable<NamedId>> GetRegions(CostElementContext context)
        {
            IEnumerable<NamedId> regions = null;

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var costElementMeta = costBlockMeta.DomainMeta.CostElements[context.CostElementId];

            if (costElementMeta.RegionInput != null)
            {
                if (costBlockMeta.InputLevelFields[costElementMeta.RegionInput.Id].ReferenceMeta is CountryEntityMeta)
                {
                    var userCountries = this.userService.GetCurrentUserCountries().ToArray();
                    if (userCountries.Length == 0)
                    {
                        regions = await GetRegions();
                    }
                    else
                    {
                        regions = userCountries.Select(country => new NamedId { Id = country.Id, Name = country.Name }).ToArray();
                    }
                }
                else
                {
                    regions = await GetRegions();
                }
            }

            return regions;

            async Task<IEnumerable<NamedId>> GetRegions()
            {
                return await this.sqlRepository.GetDistinctItems(
                    context.CostBlockId, 
                    context.ApplicationId, 
                    costElementMeta.RegionInput.Id,
                    isolationLevel: IsolationLevel.ReadUncommitted);
            }
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

                            var filter = filterGroup.Key ?? new Dictionary<string, long[]>();

                            var context = new CostElementContext
                            {
                                ApplicationId = editInfo.Meta.ApplicationId,
                                CostBlockId = editInfo.Meta.CostBlockId,
                                InputLevelId = inputLevelGroup.Key,
                                CostElementId = costElementGroup.Key
                            };

                            var inputRegionInfo = editInfo.Meta.DomainMeta.CostElements[costElementGroup.Key].RegionInput;
                            if (inputRegionInfo != null)
                            {
                                var inputRegionIdColumn = inputRegionInfo.Id;
                                if (filter.TryGetValue(inputRegionIdColumn, out var inputRegionValue))
                                {
                                    if (inputRegionValue.Length == 1)
                                        context.RegionInputId = inputRegionValue[0];
                                    else
                                        throw new System.Exception($"{nameof(context.RegionInputId)} must have single value.");
                                }
                                else if (inputRegionInfo.Id == context.InputLevelId)
                                {
                                    if (editItems.Length == 1)
                                        context.RegionInputId = editItems[0].Id;
                                    else
                                        throw new System.Exception($"{inputRegionInfo.Id} must have single value on input level {inputRegionInfo.Id}.");
                                }
                            }

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

        private async Task<IEnumerable<NamedId>> GetCoordinateItemsByPorfolio(CostElementContext context, string coordinateId)
        {
            IEnumerable<NamedId> items;

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(context);
            var coordinateField = costBlockMeta.GetDomainCoordinateField(context.CostElementId, coordinateId);
            var portfolioField = this.meta.LocalPortfolio.GetFieldByReferenceMeta(coordinateField.ReferenceMeta);

            var userCountries = this.userService.GetCurrentUserCountries();
            var costBlockFilter = this.costBlockFilterBuilder.BuildRegionFilter(context, userCountries).Convert();
            var coordinateFilter = this.costBlockFilterBuilder.BuildCoordinateItemsFilter(costBlockMeta, context.CostElementId, coordinateId);

            if (portfolioField == null)
            {
                var joinConditions = new List<ConditionHelper>();
                var portfolioReferenceFields = this.meta.LocalPortfolio.ReferenceFields.ToDictionary(field => field.ReferenceMeta);

                foreach (var coordinateReferenceField in coordinateField.ReferenceMeta.ReferenceFields)
                {
                    if (portfolioReferenceFields.TryGetValue(coordinateReferenceField.ReferenceMeta, out var portfolioRefernceField))
                    {
                        joinConditions.Add(
                            SqlOperators.Equals(
                                new ColumnInfo(coordinateReferenceField.Name, coordinateField.ReferenceMeta.Name),
                                new ColumnInfo(portfolioRefernceField.Name, this.meta.LocalPortfolio.Name)));
                    }
                }

                if (joinConditions.Count > 0)
                {
                    var portfolioFilter = GetPortfolioFilter();
                    var filters = new List<FilterInfo>();

                    if (portfolioFilter != null)
                    {
                        filters.Add(new FilterInfo(portfolioFilter, this.meta.LocalPortfolio.Name));
                    }

                    if (coordinateFilter != null)
                    {
                        filters.Add(new FilterInfo(coordinateFilter, coordinateField.ReferenceMeta.Name));
                    }

                    filters.Add(new FilterInfo(costBlockFilter, costBlockMeta.Name));

                    items = await this.sqlRepository.GetDistinctItems(new DistinctItemsInfo
                    {
                        Meta = costBlockMeta,
                        ReferenceFieldName = coordinateField.Name,
                        JoinInfos = new[]
                        {
                            new JoinInfoAdvanced(this.meta.LocalPortfolio, ConditionHelper.And(joinConditions))
                        },
                        Filters = filters
                    });
                }
                else
                {
                    items = await this.GetCoordinateItems(costBlockMeta, coordinateId, costBlockFilter, coordinateFilter);
                }
            }
            else
            {
                var countryField = costBlockMeta.GetDomainCoordinateField(context.CostElementId, MetaConstants.CountryInputLevelName);
                if (countryField == null)
                {
                    items = await this.GetCoordinateItems(costBlockMeta, coordinateId, costBlockFilter, coordinateFilter);
                }
                else
                {
                    var portfolioFilter = GetPortfolioFilter();

                    items = await this.sqlRepository.GetDistinctItems(this.meta.LocalPortfolio, portfolioField.Name, portfolioFilter);
                }
            }

            return items;

            IDictionary<string, IEnumerable<object>> GetPortfolioFilter()
            {
                Dictionary<string, IEnumerable<object>> portfolioFilter = null;

                var countryMeta = this.meta.GetCountryEntityMeta();
                var portfolioCountryField = this.meta.LocalPortfolio.GetFieldByReferenceMeta(countryMeta);

                if (portfolioCountryField != null && costBlockFilter.TryGetValue(MetaConstants.CountryInputLevelName, out var countryIds))
                {
                    portfolioFilter = new Dictionary<string, IEnumerable<object>>
                    {
                        [portfolioCountryField.Name] = countryIds
                    };
                }

                return portfolioFilter;
            }
        }

        private async Task<IEnumerable<NamedId>> GetCoordinateItems(
            CostBlockEntityMeta costBlockMeta,
            string referenceFieldName,
            IDictionary<string, IEnumerable<object>> costBlockFilter,
            IDictionary<string, IEnumerable<object>> referenceFilter)
        {
            var notDeletedCondition = CostBlockQueryHelper.BuildNotDeletedCondition(costBlockMeta, costBlockMeta.Name);

            return
                await this.sqlRepository.GetDistinctItems(
                    costBlockMeta,
                    referenceFieldName,
                    costBlockFilter,
                    referenceFilter,
                    notDeletedCondition,
                    IsolationLevel.ReadUncommitted);
        }

        private async Task<CostBlockHistory[]> Update(
            EditInfo[] editInfos, 
            ApprovalOption approvalOption, 
            EditorType editorType, 
            IEnumerable<EditItemContext> editItemContexts,
            User currentUser = null)
        {
            CostBlockHistory[] histories;

            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    await this.costBlockRepository.Update(editInfos);

                    histories = await this.costBlockHistoryService.Save(editItemContexts, approvalOption, editorType, currentUser);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }

            return histories;
        }
    }
}
