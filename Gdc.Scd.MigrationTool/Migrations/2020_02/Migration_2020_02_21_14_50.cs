using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_21_14_50 //: IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<CostBlockHistory> costBlockHistoryRepository;

        private readonly IRepository<Country> countryRepository;

        private readonly DomainEnitiesMeta enitiesMeta;

        public int Number => 159;

        public string Description => "Change region input for 'Sar' cost element";

        public Migration_2020_02_21_14_50(
            IRepositorySet repositorySet,
            IRepository<CostBlockHistory> costBlockHistoryRepository,
            IRepository<Country> countryRepository,
            DomainEnitiesMeta enitiesMeta)
        {
            this.repositorySet = repositorySet;
            this.costBlockHistoryRepository = costBlockHistoryRepository;
            this.countryRepository = countryRepository;
            this.enitiesMeta = enitiesMeta;
        }

        public void Execute()
        {
            var oldCostBlockHistories =
                this.costBlockHistoryRepository.GetAll()
                                               .Where(
                                                    history =>
                                                       history.Context.ApplicationId == MetaConstants.HardwareSchema &&
                                                       history.Context.CostBlockId == "ServiceSupportCost" &&
                                                       history.Context.CostElementId == "Sar")
                                               .ToArray();

            var relatedHistoryClusterPla =
                (RelatedItemsHistoryEntityMeta)this.enitiesMeta.GetEntityMeta(
                    "ClusterPla",
                    MetaConstants.HistoryRelatedItemsSchema);

            var relatedHistoryCountry =
                (RelatedItemsHistoryEntityMeta)this.enitiesMeta.GetEntityMeta(
                    MetaConstants.CountryInputLevelName,
                    MetaConstants.HistoryRelatedItemsSchema);

            var serviceSupportCostBlock = 
                this.enitiesMeta.GetCostBlockEntityMeta(MetaConstants.HardwareSchema, "ServiceSupportCost");

            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    foreach (var oldCostBlockHistory in oldCostBlockHistories)
                    {
                        var countryIds =
                            this.countryRepository.GetAll()
                                                  .Where(x => x.ClusterRegionId == oldCostBlockHistory.Context.RegionInputId)
                                                  .Select(x => x.Id)
                                                  .ToArray();

                        var newCostBlockHistories = this.SaveNewCostBlockHistories(oldCostBlockHistory, countryIds);

                        foreach (var newCostBlockHistory in newCostBlockHistories)
                        {
                            this.SaveNewRelatedHistoryClusterPla(oldCostBlockHistory, newCostBlockHistory, relatedHistoryClusterPla);
                            this.SaveNewHistoryValues(oldCostBlockHistory, newCostBlockHistory, serviceSupportCostBlock.HistoryMeta);
                            this.SaveNewRelatedHistory(newCostBlockHistory, relatedHistoryClusterPla, relatedHistoryCountry);
                        }

                        this.SaveNewRelatedHistoryCountry(newCostBlockHistories, relatedHistoryCountry);
                    }
                    
                    this.DeleteOldHistory(oldCostBlockHistories, serviceSupportCostBlock.HistoryMeta);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    throw ex;
                }

                transaction.Commit();
            }
        }

        private CostBlockHistory[] SaveNewCostBlockHistories(CostBlockHistory oldCostBlockHistory, long[] countryIds)
        {
            var newCostBlockHistories = new List<CostBlockHistory>();

            foreach (var countryId in countryIds)
            {
                var newCostBlockHistory = (CostBlockHistory)oldCostBlockHistory.Clone();

                newCostBlockHistory.Id = 0;
                newCostBlockHistory.Context.RegionInputId = countryId;

                this.costBlockHistoryRepository.Save(newCostBlockHistory);

                newCostBlockHistories.Add(newCostBlockHistory);
            }

            this.repositorySet.Sync();

            return newCostBlockHistories.ToArray();
        }

        private void SaveNewRelatedHistoryCountry(
            IEnumerable<CostBlockHistory> newCostBlockHistories,
            RelatedItemsHistoryEntityMeta relatedHistoryCountry)
        {
            var relatHistoryedItemsCountryData =
                newCostBlockHistories.Select(costBlockHistory => new object[] 
                                     { 
                                        costBlockHistory.Id, 
                                        costBlockHistory.Context.RegionInputId.Value 
                                     });

            var query =
                Sql.Insert(
                        relatedHistoryCountry,
                        relatedHistoryCountry.CostBlockHistoryField.Name,
                        relatedHistoryCountry.RelatedItemField.Name)
                   .Values(relatHistoryedItemsCountryData);

            this.repositorySet.ExecuteSql(query);
        }

        private void SaveNewRelatedHistoryClusterPla(
            CostBlockHistory oldCostBlockHistory,
            CostBlockHistory newCostBlockHistory, 
            RelatedItemsHistoryEntityMeta relatedHistoryClusterPla)
        {
            var query =
                Sql.Insert(
                        relatedHistoryClusterPla,
                        relatedHistoryClusterPla.CostBlockHistoryField.Name,
                        relatedHistoryClusterPla.RelatedItemField.Name)
                   .Query(
                        Sql.Select(
                                new QueryColumnInfo(
                                    new ParameterSqlBuilder(newCostBlockHistory.Id),
                                    relatedHistoryClusterPla.CostBlockHistoryField.Name),
                                new ColumnInfo(relatedHistoryClusterPla.RelatedItemField.Name))
                           .From(relatedHistoryClusterPla, "t")
                           .Where(new Dictionary<string, IEnumerable<object>>
                           {
                               [relatedHistoryClusterPla.CostBlockHistoryField.Name] = new object[] { oldCostBlockHistory.Id }
                           }));

            this.repositorySet.ExecuteSql(query);
        }

        private void SaveNewHistoryValues(
            CostBlockHistory oldCostBlockHistory,
            CostBlockHistory newCostBlockHistory, 
            CostBlockValueHistoryEntityMeta valueHistory)
        {
            //var query =
            //    Sql.Update(
            //            valueHistory,
            //            new ValueUpdateColumnInfo(valueHistory.CostBlockHistoryField, newCostBlockHistory.Id))
            //       .From(valueHistory)
            //       .Where(new Dictionary<string, IEnumerable<object>>
            //       {
            //           [valueHistory.CostBlockHistoryField.Name] = new object[] { oldCostBlockHistory.Id }
            //       });

            //this.repositorySet.ExecuteSql(query);

            const string SarCostElement = "Sar";

            var clusterPlaField = valueHistory.InputLevelFields["ClusterPla"];

            var query =
                Sql.Insert(
                        valueHistory,
                        valueHistory.CostBlockHistoryField.Name,
                        clusterPlaField.Name,
                        SarCostElement)
                   .Query(
                        Sql.Select(
                                new QueryColumnInfo(
                                    new ParameterSqlBuilder(newCostBlockHistory.Id),
                                    valueHistory.CostBlockHistoryField.Name),
                                new ColumnInfo(clusterPlaField.Name),
                                new ColumnInfo(SarCostElement))
                           .From(valueHistory, "t")
                           .Where(new Dictionary<string, IEnumerable<object>>
                           {
                               [valueHistory.CostBlockHistoryField.Name] = new object[] { oldCostBlockHistory.Id }
                           }));

            this.repositorySet.ExecuteSql(query);
        }

        private void SaveNewRelatedHistory(
            CostBlockHistory newCostBlockHistory,
            RelatedItemsHistoryEntityMeta relatedHistoryClusterPla,
            RelatedItemsHistoryEntityMeta relatedHistoryCountry)
        {
            var queries =
                this.enitiesMeta.RelatedItemsHistories.Where(
                    history =>
                        history != relatedHistoryClusterPla && history != relatedHistoryCountry)
                                                      .Select(
                    history =>
                        Sql.Insert(
                                history,
                                history.CostBlockHistoryField.Name,
                                history.RelatedItemField.Name)
                           .Values(newCostBlockHistory.Id, null));

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }

        private void DeleteOldHistory(CostBlockHistory[] oldCostBlockHistories, CostBlockValueHistoryEntityMeta valueHistory)
        {
            var oldCostBlockHistoryIds = 
                oldCostBlockHistories.Select(history => history.Id)
                                     .Cast<object>()
                                     .ToArray();
            var queries =
                this.enitiesMeta.RelatedItemsHistories.Select(
                    history => BuildDeleteRelatedItemsHistoryQuery(history, history.CostBlockHistoryField))
                                                      .ToList();

            queries.Add(BuildDeleteRelatedItemsHistoryQuery(valueHistory, valueHistory.CostBlockHistoryField));

            this.repositorySet.ExecuteSql(Sql.Queries(queries));

            foreach (var oldCostBlockHistory in oldCostBlockHistories)
            {
                this.costBlockHistoryRepository.Delete(oldCostBlockHistory.Id);
            }

            this.repositorySet.Sync();

            SqlHelper BuildDeleteRelatedItemsHistoryQuery(BaseEntityMeta meta, FieldMeta costBlockHistoryField)
            {
                return
                    Sql.Delete(meta)
                       .Where(new Dictionary<string, IEnumerable<object>>
                       {
                           [costBlockHistoryField.Name] = oldCostBlockHistoryIds
                       });
            }
        }
    }
}
