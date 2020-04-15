using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.MigrationTool.Impl;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_04_10_14_57 : AutoNumberMigrationAction
    {
        private readonly IDataMigrator dataMigrator;
        private readonly IMetaProvider metaProvider;
        private readonly IDomainService<Availability> availabilityService;
        private readonly ICostBlockRepository costBlockRepository;
        private readonly IRepositorySet repositorySet;

        public override string Description => "Cost blocks split";

        public Migration_2020_04_10_14_57(
            IDataMigrator dataMigrator, 
            IMetaProvider metaProvider, 
            IDomainService<Availability> availabilityService,
            ICostBlockRepository costBlockRepository,
            IRepositorySet repositorySet)
        {
            this.dataMigrator = dataMigrator;
            this.metaProvider = metaProvider;
            this.availabilityService = availabilityService;
            this.costBlockRepository = costBlockRepository;
            this.repositorySet = repositorySet;
        }

        public override void Execute()
        {
            var oldMeta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_04_10_14_57_old");
            var newMeta = this.metaProvider.GetArchiveEntitiesMeta("DomainConfig_2020_04_10_14_57_new");

            this.SplitFieldServiceCost(oldMeta, newMeta);
            this.SplitProActive(oldMeta, newMeta);
            this.CalculationUpdate();
        }

        private void SplitFieldServiceCost(DomainEnitiesMeta oldMeta, DomainEnitiesMeta newMeta)
        {
            const string FieldServiceCost = "FieldServiceCost";
            const string TimeAndMaterialShare = "TimeAndMaterialShare";
            const string FieldServiceTimeCalc = "FieldServiceTimeCalc";
            const string FieldServiceCalc = "FieldServiceCalc";
            const string OohUpliftFactor = "OohUpliftFactor";
            const string UpliftFactor = "UpliftFactor";

            var oldCostBlock = oldMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost];

            AddAvailabilityColumn();

            var newCostBlocks = newMeta.CostBlocks.GetSome(MetaConstants.HardwareSchema, FieldServiceCost);

            this.dataMigrator.SplitCostBlock(oldCostBlock, newCostBlocks, newMeta, true);

            this.costBlockRepository.UpdateByCoordinates(newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, OohUpliftFactor]);

            this.dataMigrator.DropTable(FieldServiceTimeCalc, MetaConstants.HardwareSchema);
            this.dataMigrator.DropTable(FieldServiceCalc, MetaConstants.HardwareSchema);

            var ignoreCoordinates = new[] { "Pla", "CentralContractGroup" };

            this.dataMigrator.CreateCostBlockView(
                MetaConstants.HardwareSchema,
                FieldServiceCalc,
                new[] 
                {
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "RepairTime"],
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "TravelTime"],
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "TravelCost"],
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "LabourCost"],
                },
                ignoreCoordinates: ignoreCoordinates);

            var timeAndMaterialShareCostBlock = newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, TimeAndMaterialShare];

            this.dataMigrator.CreateCostBlockView(
                MetaConstants.HardwareSchema,
                FieldServiceTimeCalc,
                new[]
                {
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "PerformanceRate"],
                    timeAndMaterialShareCostBlock,
                },
                this.BuildNormColumns(timeAndMaterialShareCostBlock, TimeAndMaterialShare),
                ignoreCoordinates);

            this.dataMigrator.CreateCostBlockView(
                MetaConstants.HardwareSchema,
                UpliftFactor,
                new[]
                {
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, OohUpliftFactor],
                },
                ignoreCoordinates: ignoreCoordinates);

            void AddAvailabilityColumn()
            {
                const string Availability = "Availability";

                var availabilitityField = oldCostBlock.DependencyFields[Availability];
                var availabilitity = this.availabilityService.GetAll().Where(item => item.Name == "24x7").First();

                availabilitityField.DefaultValue = availabilitity.Id;

                this.dataMigrator.AddColumn(oldCostBlock, availabilitityField.Name);

                availabilitityField.DefaultValue = null;
                availabilitityField.IsNullOption = true;

                dataMigrator.AddColumn(oldCostBlock.HistoryMeta, availabilitityField.Name);

                var avHistoryMeta = oldMeta.GetRelatedItemsHistoryMeta(Availability);
                var historyMeta = oldMeta.CostBlockHistory;

                var query =
                    Sql.Insert(avHistoryMeta, avHistoryMeta.CostBlockHistoryField.Name, avHistoryMeta.RelatedItemField.Name)
                       .Query(
                            Sql.Select(
                                    new ColumnInfo(historyMeta.IdField, historyMeta, avHistoryMeta.CostBlockHistoryField.Name), 
                                    new QueryColumnInfo(new ParameterSqlBuilder(availabilitity.Id), avHistoryMeta.RelatedItemField.Name))
                               .From(historyMeta)
                               .Where(new Dictionary<string, IEnumerable<object>>
                               {
                                   [historyMeta.ContextApplicationIdField.Name] = new[] { MetaConstants.HardwareSchema },
                                   [historyMeta.ContextCostBlockIdField.Name] = new[] { FieldServiceCost },
                                   [historyMeta.ContextCostElementIdField.Name] = new[] { UpliftFactor }
                               }));

                this.repositorySet.ExecuteSql(query);
            }
        }

        private void SplitProActive(DomainEnitiesMeta oldMeta, DomainEnitiesMeta newMeta)
        {
            const string ProActive = "ProActive";

            var oldCostBlock = oldMeta.CostBlocks[MetaConstants.HardwareSchema, ProActive];
            var newCostBlocks = newMeta.CostBlocks.GetSome(MetaConstants.HardwareSchema, ProActive).ToArray();

            this.dataMigrator.DropView("ProActiveView", MetaConstants.HardwareSchema);
            this.dataMigrator.SplitCostBlock(oldCostBlock, newCostBlocks, newMeta, true);
            this.dataMigrator.CreateCostBlockView(MetaConstants.HardwareSchema, ProActive, newCostBlocks);

            //CREATE VIEW [Hardware].[ProActiveView]
            this.repositorySet.ExecuteSql(@"
CREATE VIEW [Hardware].[ProActiveView] as 
    with ProActiveCte as 
    (
        select pro.Country,
               pro.Wg,
               sla.Id as ProActiveSla,
               (pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate) as LocalRemoteAccessSetup,
               (pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved) as LocalRemoteAccessSetup_Approved,

               (pro.LocalRegularUpdateReadyEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalRegularUpdateReadyRepetition) as LocalRegularUpdate,

               (pro.LocalRegularUpdateReadyEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalRegularUpdateReadyRepetition) as LocalRegularUpdate_Approved,

               (pro.LocalPreparationShcEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalPreparationShcRepetition) as LocalPreparation,

               (pro.LocalPreparationShcEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalPreparationShcRepetition) as LocalPreparation_Approved,

               (pro.LocalRemoteShcCustomerBriefingEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalRemoteShcCustomerBriefingRepetition) as LocalRemoteCustomerBriefing,

               (pro.LocalRemoteShcCustomerBriefingEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalRemoteShcCustomerBriefingRepetition) as LocalRemoteCustomerBriefing_Approved,

               (pro.LocalOnsiteShcCustomerBriefingEffort * 
                pro.OnSiteHourlyRate * 
                sla.LocalOnsiteShcCustomerBriefingRepetition) as LocalOnsiteCustomerBriefing,

               (pro.LocalOnsiteShcCustomerBriefingEffort_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.LocalOnsiteShcCustomerBriefingRepetition) as LocalOnsiteCustomerBriefing_Approved,

               (pro.TravellingTime * 
                pro.OnSiteHourlyRate * 
                sla.TravellingTimeRepetition) as Travel,

               (pro.TravellingTime_Approved * 
                pro.OnSiteHourlyRate_Approved * 
                sla.TravellingTimeRepetition) as Travel_Approved,

               (pro.CentralExecutionShcReportCost * 
                sla.CentralExecutionShcReportRepetition) as CentralExecutionReport,

               (pro.CentralExecutionShcReportCost_Approved * 
                sla.CentralExecutionShcReportRepetition) as CentralExecutionReport_Approved

        from Hardware.ProActive pro, 
             Dependencies.ProActiveSla sla
    )
    select  pro.Country,
            pro.Wg,
            pro.ProActiveSla,

            pro.LocalPreparation,
            pro.LocalPreparation_Approved,

            pro.LocalRegularUpdate,
            pro.LocalRegularUpdate_Approved,

            pro.LocalRemoteCustomerBriefing,
            pro.LocalRemoteCustomerBriefing_Approved,

            pro.LocalOnsiteCustomerBriefing,
            pro.LocalOnsiteCustomerBriefing_Approved,

            pro.Travel,
            pro.Travel_Approved,

            pro.CentralExecutionReport,
            pro.CentralExecutionReport_Approved,

           pro.LocalRemoteAccessSetup as Setup,
           pro.LocalRemoteAccessSetup_Approved  as Setup_Approved,

           (pro.LocalPreparation + 
            pro.LocalRegularUpdate + 
            pro.LocalRemoteCustomerBriefing +
            pro.LocalOnsiteCustomerBriefing +
            pro.Travel +
            pro.CentralExecutionReport) as Service,
       
           (pro.LocalPreparation_Approved + 
            pro.LocalRegularUpdate_Approved + 
            pro.LocalRemoteCustomerBriefing_Approved +
            pro.LocalOnsiteCustomerBriefing_Approved +
            pro.Travel_Approved +
            pro.CentralExecutionReport_Approved) as Service_Approved

    from ProActiveCte pro;
");
        }

        private void CalculationUpdate()
        {
            //[Hardware].[CalcByFieldServicePerYear]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[CalcByFieldServicePerYear]
(
	@timeAndMaterialShare_norm FLOAT,
	@travelCost FLOAT,
	@labourCost FLOAT,
	@performanceRate FLOAT,
	@exchangeRate FLOAT,
	@travelTime FLOAT,
	@repairTime FLOAT,
	@onsiteHourlyRates FLOAT,
	@upliftFactor FLOAT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @result FLOAT

	SET @result = 
		(1 - @TimeAndMaterialShare_norm) * (@travelCost + @labourCost + @performanceRate) / @exchangeRate + 
        @timeAndMaterialShare_norm * ((@travelTime + @repairTime) * @onsiteHourlyRates + @performanceRate / @exchangeRate) 

	IF @upliftFactor IS NOT NULL
		SET @result = @result * (1 + @upliftFactor / 100)

	RETURN @result
END");

            //[Hardware].[CalcStdw]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[CalcStdw](
    @approved       bit = 0,
    @cnt            dbo.ListID READONLY,
    @wg             dbo.ListID READONLY
)
RETURNS @tbl TABLE  (
          CountryId                         bigint
        , Country                           nvarchar(255)
        , CurrencyId                        bigint
        , Currency                          nvarchar(255)
        , ClusterRegionId                   bigint
        , ExchangeRate                      float

        , WgId                              bigint
        , Wg                                nvarchar(255)
        , SogId                             bigint
        , Sog                               nvarchar(255)
        , ClusterPlaId                      bigint
        , RoleCodeId                        bigint

        , StdFspId                          bigint
        , StdFsp                            nvarchar(255)

        , StdWarranty                       int
        , StdWarrantyLocation               nvarchar(255)

        , AFR1                              float 
        , AFR2                              float
        , AFR3                              float
        , AFR4                              float
        , AFR5                              float
        , AFRP1                             float

        , OnsiteHourlyRates                 float
        , CanOverrideTransferCostAndPrice   bit

        --####### PROACTIVE COST ###################
        , LocalRemoteAccessSetup       float
        , LocalRegularUpdate           float
        , LocalPreparation             float
        , LocalRemoteCustomerBriefing  float
        , LocalOnsiteCustomerBriefing  float
        , Travel                       float
        , CentralExecutionReport       float

        , Fee                          float

        , MatW1                        float
        , MatW2                        float
        , MatW3                        float
        , MatW4                        float
        , MatW5                        float
        , MaterialW                    float

        , MatOow1                      float
        , MatOow2                      float
        , MatOow3                      float
        , MatOow4                      float
        , MatOow5                      float
        , MatOow1p                     float

        , MatCost1                     float
        , MatCost2                     float
        , MatCost3                     float
        , MatCost4                     float
        , MatCost5                     float
        , MatCost1P                    float

        , TaxW1                        float
        , TaxW2                        float
        , TaxW3                        float
        , TaxW4                        float
        , TaxW5                        float
        , TaxAndDutiesW                float

        , TaxOow1                      float
        , TaxOow2                      float
        , TaxOow3                      float
        , TaxOow4                      float
        , TaxOow5                      float
        , TaxOow1P                     float

        , TaxAndDuties1                float
        , TaxAndDuties2                float
        , TaxAndDuties3                float
        , TaxAndDuties4                float
        , TaxAndDuties5                float
        , TaxAndDuties1P               float

        , ServiceSupportPerYear                  float
        , ServiceSupportPerYearWithoutSar        float
        , LocalServiceStandardWarranty           float
        , LocalServiceStandardWarrantyWithoutSar float
        , LocalServiceStandardWarrantyManual     float
        
        , Credit1                      float
        , Credit2                      float
        , Credit3                      float
        , Credit4                      float
        , Credit5                      float
        , Credits                      float

        , Credit1WithoutSar            float
        , Credit2WithoutSar            float
        , Credit3WithoutSar            float
        , Credit4WithoutSar            float
        , Credit5WithoutSar            float
        , CreditsWithoutSar            float
        
        , PRIMARY KEY CLUSTERED(CountryId, WgId)
    )
AS
BEGIN

    with WgCte as (
        select wg.Id as WgId
             , wg.Name as Wg
             , wg.SogId
             , sog.Name as Sog
             , pla.ClusterPlaId
             , wg.RoleCodeId

             , case when @approved = 0 then afr.AFR1                           else afr.AFR1_Approved                       end as AFR1 
             , case when @approved = 0 then afr.AFR2                           else afr.AFR2_Approved                       end as AFR2 
             , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                       end as AFR3 
             , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                       end as AFR4 
             , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                       end as AFR5 
             , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                      end as AFRP1

        from InputAtoms.Wg wg
        left join InputAtoms.Sog sog on sog.Id = wg.SogId
        left join InputAtoms.Pla pla on pla.id = wg.PlaId
        left join Hardware.AfrYear afr on afr.Wg = wg.Id
        where wg.WgType = 1 and wg.Deactivated = 0 and (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
    )
    , CntCte as (
        select c.Id as CountryId
             , c.Name as Country
             , c.CurrencyId
             , cur.Name as Currency
             , c.ClusterRegionId
             , c.CanOverrideTransferCostAndPrice
             , er.Value as ExchangeRate 
             , isnull(case when @approved = 0 then tax.TaxAndDuties_norm  else tax.TaxAndDuties_norm_Approved end, 0) as TaxAndDutiesOrZero

        from InputAtoms.Country c
        LEFT JOIN [References].Currency cur on cur.Id = c.CurrencyId
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.Deactivated = 0
        where exists(select * from @cnt where id = c.Id)
    )
    , WgCnt as (
        select c.*, wg.*
        from CntCte c, WgCte wg
    )
    , Std as (
        select  m.*

              , case when @approved = 0 then hr.OnsiteHourlyRates                     else hr.OnsiteHourlyRates_Approved                 end / m.ExchangeRate as OnsiteHourlyRates      

              , stdw.FspId                                    as StdFspId
              , stdw.Fsp                                      as StdFsp
              , stdw.AvailabilityId                           as StdAvailabilityId 
              , stdw.Duration                                 as StdDuration
              , stdw.DurationId                               as StdDurationId
              , stdw.DurationValue                            as StdDurationValue
              , stdw.IsProlongation                           as StdIsProlongation
              , stdw.ProActiveSlaId                           as StdProActiveSlaId
              , stdw.ReactionTime_Avalability                 as StdReactionTime_Avalability
              , stdw.ReactionTime_ReactionType                as StdReactionTime_ReactionType
              , stdw.ReactionTime_ReactionType_Avalability    as StdReactionTime_ReactionType_Avalability
              , stdw.ServiceLocation                          as StdServiceLocation
              , stdw.ServiceLocationId                        as StdServiceLocationId

              , case when @approved = 0 then mcw.MaterialCostIw                      else mcw.MaterialCostIw_Approved                    end as MaterialCostWarranty
              , case when @approved = 0 then mcw.MaterialCostOow                     else mcw.MaterialCostOow_Approved                   end as MaterialCostOow     

              , case when @approved = 0 then msw.MarkupStandardWarranty              else msw.MarkupStandardWarranty_Approved            end / m.ExchangeRate as MarkupStandardWarranty      
              , case when @approved = 0 then msw.MarkupFactorStandardWarranty_norm   else msw.MarkupFactorStandardWarranty_norm_Approved end + 1              as MarkupFactorStandardWarranty

              --##### SERVICE SUPPORT COST #########                                                                                               
             , case when @approved = 0 then ssc.[1stLevelSupportCostsCountry]        else ssc.[1stLevelSupportCostsCountry_Approved]     end / m.ExchangeRate as [1stLevelSupportCosts] 
             , case when @approved = 0 
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0 then ssc.[2ndLevelSupportCostsLocal] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion] end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.[2ndLevelSupportCostsLocal_Approved] / m.ExchangeRate else ssc.[2ndLevelSupportCostsClusterRegion_Approved] end)
                 end as [2ndLevelSupportCosts] 
             , case when @approved = 0 then ssc.TotalIb                        else ssc.TotalIb_Approved                    end as TotalIb 
             , case when @approved = 0
                     then (case when ssc.[2ndLevelSupportCostsLocal] > 0          then ssc.TotalIbClusterPla          else ssc.TotalIbClusterPlaRegion end)
                     else (case when ssc.[2ndLevelSupportCostsLocal_Approved] > 0 then ssc.TotalIbClusterPla_Approved else ssc.TotalIbClusterPlaRegion_Approved end)
                 end as TotalIbPla
             , case when @approved = 0 then ssc.Sar else ssc.Sar_Approved end as Sar

              , case when @approved = 0 then af.Fee else af.Fee_Approved end as Fee
              , isnull(case when afEx.id is not null 
                            then (case when @approved = 0 then af.Fee else af.Fee_Approved end) 
                        end, 
                    0) as FeeOrZero

              --####### PROACTIVE COST ###################

              , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate   else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved end as LocalRemoteAccessSetup
              , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate             else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved           end as LocalRegularUpdate
              , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate                 else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved               end as LocalPreparation
              , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalRemoteCustomerBriefing
              , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate      else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved    end as LocalOnsiteCustomerBriefing
              , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate                            else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved                          end as Travel
              , case when @approved = 0 then pro.CentralExecutionShcReportCost                                    else pro.CentralExecutionShcReportCost_Approved                                           end as CentralExecutionReport

              --##### FIELD SERVICE COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0 
                     then fscStd.LabourCost + fscStd.TravelCost + isnull(fstStd.PerformanceRate, 0)
                     else fscStd.LabourCost_Approved + fscStd.TravelCost_Approved + isnull(fstStd.PerformanceRate_Approved, 0)
                 end / m.ExchangeRate as FieldServicePerYearStdw

               --##### LOGISTICS COST STANDARD WARRANTY #########                                                                                               
              , case when @approved = 0
                     then lcStd.StandardHandling + lcStd.HighAvailabilityHandling + lcStd.StandardDelivery + lcStd.ExpressDelivery + lcStd.TaxiCourierDelivery + lcStd.ReturnDeliveryFactory 
                     else lcStd.StandardHandling_Approved + lcStd.HighAvailabilityHandling_Approved + lcStd.StandardDelivery_Approved + lcStd.ExpressDelivery_Approved + lcStd.TaxiCourierDelivery_Approved + lcStd.ReturnDeliveryFactory_Approved
                 end / m.ExchangeRate as LogisticPerYearStdw

              , man.StandardWarranty / m.ExchangeRate as ManualStandardWarranty

        from WgCnt m

        LEFT JOIN Hardware.RoleCodeHourlyRates hr ON hr.Country = m.CountryId and hr.RoleCode = m.RoleCodeId and hr.Deactivated = 0

        LEFT JOIN Fsp.HwStandardWarranty stdw ON stdw.Country = m.CountryId and stdw.Wg = m.WgId 

        LEFT JOIN Hardware.ServiceSupportCost ssc ON ssc.Country = m.CountryId and ssc.ClusterPla = m.ClusterPlaId and ssc.Deactivated = 0

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw ON mcw.Country = m.CountryId and mcw.Wg = m.WgId

        LEFT JOIN Hardware.MarkupStandardWaranty msw ON msw.Country = m.CountryId AND msw.Wg = m.WgId and msw.Deactivated = 0

        LEFT JOIN Hardware.AvailabilityFeeCalc af ON af.Country = m.CountryId AND af.Wg = m.WgId 
        LEFT JOIN Admin.AvailabilityFee afEx ON afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = stdw.ReactionTimeId AND afEx.ReactionTypeId = stdw.ReactionTypeId AND afEx.ServiceLocationId = stdw.ServiceLocationId

        LEFT JOIN Hardware.ProActive pro ON pro.Country= m.CountryId and pro.Wg= m.WgId

        LEFT JOIN Hardware.FieldServiceCalc fscStd     ON fscStd.Country = stdw.Country AND fscStd.Wg = stdw.Wg AND fscStd.ServiceLocation = stdw.ServiceLocationId 
        LEFT JOIN Hardware.FieldServiceTimeCalc fstStd ON fstStd.Country = stdw.Country AND fstStd.Wg = stdw.Wg AND fstStd.ReactionTimeType = stdw.ReactionTime_ReactionType 

        LEFT JOIN Hardware.LogisticsCosts lcStd        ON lcStd.Country  = stdw.Country AND lcStd.Wg = stdw.Wg  AND lcStd.ReactionTimeType = stdw.ReactionTime_ReactionType and lcStd.Deactivated = 0

        LEFT JOIN Hardware.StandardWarrantyManualCost man on man.CountryId = m.CountryId and man.WgId = m.WgId
    )
    , CostCte as (
        select    m.*

                , case when m.TotalIb > 0 and m.TotalIbPla > 0 then m.[1stLevelSupportCosts] / m.TotalIb + m.[2ndLevelSupportCosts] / m.TotalIbPla end as ServiceSupportPerYear

        from Std m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdDurationValue >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdDurationValue >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdDurationValue >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdDurationValue >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdDurationValue >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5

                , case when m.StdDurationValue >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdDurationValue >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdDurationValue >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdDurationValue >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdDurationValue >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1                                                  as matO1P

                , 1 - isnull(m.Sar, 0)/100 as SarCoeff
        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdDurationValue >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdDurationValue >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdDurationValue >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdDurationValue >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdDurationValue >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5

                , case when m.StdDurationValue >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdDurationValue >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdDurationValue >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdDurationValue >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdDurationValue >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5

        from CostCte2 m
    )
    , CostCte3 as (
        select   m.*

               , case when m.StdDurationValue >= 1 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
                       else 0 
                   end as LocalServiceStandardWarranty1
               , case when m.StdDurationValue >= 2 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
                       else 0 
                   end as LocalServiceStandardWarranty2
               , case when m.StdDurationValue >= 3 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
                       else 0 
                   end as LocalServiceStandardWarranty3
               , case when m.StdDurationValue >= 4 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
                       else 0 
                   end as LocalServiceStandardWarranty4
               , case when m.StdDurationValue >= 5 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, m.SarCoeff)
                       else 0 
                   end as LocalServiceStandardWarranty5

               , case when m.StdDurationValue >= 1 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR1, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR1, m.tax1, m.AFR1, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
                       else 0 
                   end as LocalServiceStandardWarranty1WithoutSar
               , case when m.StdDurationValue >= 2 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR2, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR2, m.tax2, m.AFR2, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
                       else 0 
                   end as LocalServiceStandardWarranty2WithoutSar
               , case when m.StdDurationValue >= 3 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR3, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR3, m.tax3, m.AFR3, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
                       else 0 
                   end as LocalServiceStandardWarranty3WithoutSar
               , case when m.StdDurationValue >= 4 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR4, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR4, m.tax4, m.AFR4, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
                       else 0 
                   end as LocalServiceStandardWarranty4WithoutSar
               , case when m.StdDurationValue >= 5 
                       then Hardware.CalcLocSrvStandardWarranty(m.FieldServicePerYearStdw * m.AFR5, m.ServiceSupportPerYear, m.LogisticPerYearStdw * m.AFR5, m.tax5, m.AFR5, m.FeeOrZero, m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty, 1)
                       else 0 
                   end as LocalServiceStandardWarranty5WithoutSar

        from CostCte2_2 m
    )
    insert into @tbl(
                 CountryId                    
               , Country                      
               , CurrencyId                   
               , Currency                     
               , ClusterRegionId              
               , ExchangeRate                 
               
               , WgId                         
               , Wg                           
               , SogId                        
               , Sog                          
               , ClusterPlaId                 
               , RoleCodeId                   

               , StdFspId
               , StdFsp  

               , StdWarranty         
               , StdWarrantyLocation 
               
               , AFR1                         
               , AFR2                         
               , AFR3                         
               , AFR4                         
               , AFR5                         
               , AFRP1                        

               , OnsiteHourlyRates

               , CanOverrideTransferCostAndPrice

               , LocalRemoteAccessSetup     
               , LocalRegularUpdate         
               , LocalPreparation           
               , LocalRemoteCustomerBriefing
               , LocalOnsiteCustomerBriefing
               , Travel                     
               , CentralExecutionReport     
               
               , Fee                          

               , MatW1                
               , MatW2                
               , MatW3                
               , MatW4                
               , MatW5                
               , MaterialW            
               
               , MatOow1              
               , MatOow2              
               , MatOow3              
               , MatOow4              
               , MatOow5              
               , MatOow1p             
               
               , MatCost1             
               , MatCost2             
               , MatCost3             
               , MatCost4             
               , MatCost5             
               , MatCost1P            
               
               , TaxW1                
               , TaxW2                
               , TaxW3                
               , TaxW4                
               , TaxW5                
               , TaxAndDutiesW        
               
               , TaxOow1              
               , TaxOow2              
               , TaxOow3              
               , TaxOow4              
               , TaxOow5              
               , TaxOow1P             
               
               , TaxAndDuties1        
               , TaxAndDuties2        
               , TaxAndDuties3        
               , TaxAndDuties4        
               , TaxAndDuties5        
               , TaxAndDuties1P       

               , ServiceSupportPerYear
               , ServiceSupportPerYearWithoutSar
               , LocalServiceStandardWarranty
               , LocalServiceStandardWarrantyWithoutSar
               , LocalServiceStandardWarrantyManual
               
               , Credit1                      
               , Credit2                      
               , Credit3                      
               , Credit4                      
               , Credit5                      
               , Credits        
               
               , Credit1WithoutSar                      
               , Credit2WithoutSar                      
               , Credit3WithoutSar                      
               , Credit4WithoutSar                      
               , Credit5WithoutSar                      
               , CreditsWithoutSar      
        )
    select    m.CountryId                    
            , m.Country                      
            , m.CurrencyId                   
            , m.Currency                     
            , m.ClusterRegionId              
            , m.ExchangeRate                 

            , m.WgId        
            , m.Wg          
            , m.SogId       
            , m.Sog         
            , m.ClusterPlaId
            , m.RoleCodeId  

            , m.StdFspId
            , m.StdFsp
            , m.StdDurationValue
            , m.StdServiceLocation

            , m.AFR1 
            , m.AFR2 
            , m.AFR3 
            , m.AFR4 
            , m.AFR5 
            , m.AFRP1

            , m.OnsiteHourlyRates
            , m.CanOverrideTransferCostAndPrice

            , m.LocalRemoteAccessSetup     
            , m.LocalRegularUpdate         
            , m.LocalPreparation           
            , m.LocalRemoteCustomerBriefing
            , m.LocalOnsiteCustomerBriefing
            , m.Travel                     
            , m.CentralExecutionReport     

            , m.Fee

            , m.mat1                
            , m.mat2                
            , m.mat3                
            , m.mat4                
            , m.mat5                
            , m.mat1 + m.mat2 + m.mat3 + m.mat4 + m.mat5 as MaterialW
            
            , m.matO1              
            , m.matO2              
            , m.matO3              
            , m.matO4              
            , m.matO5              
            , m.matO1P
            
            , m.mat1  + m.matO1  as matCost1
            , m.mat2  + m.matO2  as matCost2
            , m.mat3  + m.matO3  as matCost3
            , m.mat4  + m.matO4  as matCost4
            , m.mat5  + m.matO5  as matCost5
            , m.matO1P           as matCost1P
            
            , m.tax1                
            , m.tax2                
            , m.tax3                
            , m.tax4                
            , m.tax5                
            , m.tax1 + m.tax2 + m.tax3 + m.tax4 + m.tax5 as TaxAndDutiesW
            
            , m.TaxAndDutiesOrZero * m.matO1              
            , m.TaxAndDutiesOrZero * m.matO2              
            , m.TaxAndDutiesOrZero * m.matO3              
            , m.TaxAndDutiesOrZero * m.matO4              
            , m.TaxAndDutiesOrZero * m.matO5              
            , m.TaxAndDutiesOrZero * m.matO1P             
            
            , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
            , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
            , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
            , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
            , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
            , m.TaxAndDutiesOrZero * m.matO1P as TaxAndDuties1P

            , case when  m.Sar is null then m.ServiceSupportPerYear else m.ServiceSupportPerYear * m.Sar / 100 end as ServiceSupportPerYear
            , m.ServiceSupportPerYear as ServiceSupportPerYearWithoutSar

            , m.LocalServiceStandardWarranty1 + m.LocalServiceStandardWarranty2 + m.LocalServiceStandardWarranty3 + m.LocalServiceStandardWarranty4 + m.LocalServiceStandardWarranty5 as LocalServiceStandardWarranty
            , m.LocalServiceStandardWarranty1WithoutSar + m.LocalServiceStandardWarranty2WithoutSar + m.LocalServiceStandardWarranty3WithoutSar + m.LocalServiceStandardWarranty4WithoutSar + m.LocalServiceStandardWarranty5WithoutSar as LocalServiceStandardWarrantyWithoutSar
            , m.ManualStandardWarranty as LocalServiceStandardWarrantyManual

            , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
            , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
            , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
            , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
            , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5

            , m.mat1 + m.LocalServiceStandardWarranty1   +
                m.mat2 + m.LocalServiceStandardWarranty2 +
                m.mat3 + m.LocalServiceStandardWarranty3 +
                m.mat4 + m.LocalServiceStandardWarranty4 +
                m.mat5 + m.LocalServiceStandardWarranty5 as Credit

            , m.mat1 + m.LocalServiceStandardWarranty1WithoutSar as Credit1WithoutSar 
            , m.mat2 + m.LocalServiceStandardWarranty2WithoutSar as Credit2WithoutSar 
            , m.mat3 + m.LocalServiceStandardWarranty3WithoutSar as Credit3WithoutSar 
            , m.mat4 + m.LocalServiceStandardWarranty4WithoutSar as Credit4WithoutSar 
            , m.mat5 + m.LocalServiceStandardWarranty5WithoutSar as Credit5WithoutSar 

            , m.mat1 + m.LocalServiceStandardWarranty1WithoutSar   +
                m.mat2 + m.LocalServiceStandardWarranty2WithoutSar +
                m.mat3 + m.LocalServiceStandardWarranty3WithoutSar +
                m.mat4 + m.LocalServiceStandardWarranty4WithoutSar +
                m.mat5 + m.LocalServiceStandardWarranty5WithoutSar as CreditWithoutSar 

    from CostCte3 m;

    RETURN;
END");

            //[Hardware].[GetCalcMember]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[GetCalcMember] (
    @approved       bit,
    @cnt            dbo.ListID readonly,
    @wg             dbo.ListID readonly,
    @av             dbo.ListID readonly,
    @dur            dbo.ListID readonly,
    @reactiontime   dbo.ListID readonly,
    @reactiontype   dbo.ListID readonly,
    @loc            dbo.ListID readonly,
    @pro            dbo.ListID readonly,
    @lastid         bigint,
    @limit          int
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.rownum
            , m.Id

            --SLA

            , m.Fsp
            , m.CountryId          
            , std.Country
            , std.CurrencyId
            , std.Currency
            , std.ExchangeRate
            , m.WgId
            , std.Wg
            , std.SogId
            , std.Sog
            , m.DurationId
            , dur.Name             as Duration
            , dur.Value            as Year
            , dur.IsProlongation   as IsProlongation
            , m.AvailabilityId
            , av.Name              as Availability
            , m.ReactionTimeId
            , rtime.Name           as ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

            , std.StdWarranty
            , std.StdWarrantyLocation

            --Cost values

            , std.AFR1  
            , std.AFR2  
            , std.AFR3  
            , std.AFR4  
            , std.AFR5  
            , std.AFRP1 

            , std.MatCost1
            , std.MatCost2
            , std.MatCost3
            , std.MatCost4
            , std.MatCost5
            , std.MatCost1P

            , std.MatOow1 
            , std.MatOow2 
            , std.MatOow3 
            , std.MatOow4 
            , std.MatOow5 
            , std.MatOow1p

            , std.MaterialW

            , std.TaxAndDuties1
            , std.TaxAndDuties2
            , std.TaxAndDuties3
            , std.TaxAndDuties4
            , std.TaxAndDuties5
            , std.TaxAndDuties1P

            , std.TaxOow1 
            , std.TaxOow2 
            , std.TaxOow3 
            , std.TaxOow4 
            , std.TaxOow5 
            , std.TaxOow1P
            
            , std.TaxAndDutiesW

            , ISNULL(case when @approved = 0 then r.Cost else r.Cost_approved end, 0) as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 
				   then 
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm, 
							fsc.TravelCost, 
							fsc.LabourCost, 
							fst.PerformanceRate, 
							std.ExchangeRate,
							fsc.TravelTime,
							fsc.repairTime,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor)
					else
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm_Approved, 
							fsc.TravelCost_Approved, 
							fsc.LabourCost_Approved, 
							fst.PerformanceRate_Approved, 
							std.ExchangeRate,
							fsc.TravelTime_Approved,
							fsc.repairTime_Approved,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor_Approved)

               end as FieldServicePerYear

            --##### SERVICE SUPPORT COST #########                                                                                               
            , case when dur.IsProlongation = 1 then std.ServiceSupportPerYearWithoutSar else std.ServiceSupportPerYear end as ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 
                   then lc.ExpressDelivery          +
                        lc.HighAvailabilityHandling +
                        lc.StandardDelivery         +
                        lc.StandardHandling         +
                        lc.ReturnDeliveryFactory    +
                        lc.TaxiCourierDelivery      
                   else lc.ExpressDelivery_Approved          +
                        lc.HighAvailabilityHandling_Approved +
                        lc.StandardDelivery_Approved         +
                        lc.StandardHandling_Approved         +
                        lc.ReturnDeliveryFactory_Approved    +
                        lc.TaxiCourierDelivery_Approved     
                end / std.ExchangeRate as LogisticPerYear

                                                                                                                       
            , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
                    else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
                end / std.ExchangeRate as MarkupOtherCost                      
            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
                    else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
                end as MarkupFactorOtherCost                

            --####### PROACTIVE COST ###################
            , case when proSla.Name = '0' 
                    then 0 --we don't calc proactive(none)
                    else std.LocalRemoteAccessSetup + dur.Value * (
                                      std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
                                    + std.LocalPreparation * proSla.LocalPreparationShcRepetition                      
                                    + std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
                                    + std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
                                    + std.Travel * proSla.TravellingTimeRepetition                                     
                                    + std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
                                )
                end as ProActive

            --We don't use STDW and credits for Prolongation
            , case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarranty       end as LocalServiceStandardWarranty
            , case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarrantyManual end as LocalServiceStandardWarrantyManual

            , std.Credit1 
            , std.Credit2 
            , std.Credit3 
            , std.Credit4 
            , std.Credit5 
            , case when dur.IsProlongation <> 1 then std.Credits end as Credits

            --########## MANUAL COSTS ################
            , man.ListPrice          / std.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / std.ExchangeRate as DealerPrice                 
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / std.ExchangeRate) end as ServiceTCManual                   
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP    / std.ExchangeRate) end as ReActiveTPManual                   
            , man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  

            , man.ReleaseDate                           as ReleaseDate
            , u2.Name                                   as ReleaseUserName
            , u2.Email                                  as ReleaseUserEmail

            , man.ChangeDate                            
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

    FROM Hardware.CalcStdw(@approved, @cnt, @wg) std 

    INNER JOIN Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on std.CountryId = m.CountryId and std.WgId = m.WgId 

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.ReinsuranceCalc r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType
    LEFT JOIN Hardware.UpliftFactor ON UpliftFactor.Country = m.CountryId AND UpliftFactor.Wg = m.WgId AND UpliftFactor.[Availability] = m.AvailabilityId

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId
)");

            //[Hardware].[GetCalcMember2]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[GetCalcMember2] (
    @approved       bit,
    @cnt            dbo.ListID readonly,
    @fsp            nvarchar(255),
    @hasFsp         bit,
    @wg             dbo.ListID readonly,
    @av             dbo.ListID readonly,
    @dur            dbo.ListID readonly,
    @reactiontime   dbo.ListID readonly,
    @reactiontype   dbo.ListID readonly,
    @loc            dbo.ListID readonly,
    @pro            dbo.ListID readonly,
    @lastid         bigint,
    @limit          int
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.rownum
            , m.Id

            --SLA

            , m.Fsp
            , m.CountryId          
            , std.Country
            , std.CurrencyId
            , std.Currency
            , std.ExchangeRate
            , m.WgId
            , std.Wg
            , std.SogId
            , std.Sog
            , m.DurationId
            , dur.Name             as Duration
            , dur.Value            as Year
            , dur.IsProlongation   as IsProlongation
            , m.AvailabilityId
            , av.Name              as Availability
            , m.ReactionTimeId
            , rtime.Name           as ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

            , std.StdWarranty
            , std.StdWarrantyLocation

            --Cost values

            , std.AFR1  
            , std.AFR2  
            , std.AFR3  
            , std.AFR4  
            , std.AFR5  
            , std.AFRP1 

            , std.MatCost1
            , std.MatCost2
            , std.MatCost3
            , std.MatCost4
            , std.MatCost5
            , std.MatCost1P

            , std.MatOow1 
            , std.MatOow2 
            , std.MatOow3 
            , std.MatOow4 
            , std.MatOow5 
            , std.MatOow1p

            , std.MaterialW

            , std.TaxAndDuties1
            , std.TaxAndDuties2
            , std.TaxAndDuties3
            , std.TaxAndDuties4
            , std.TaxAndDuties5
            , std.TaxAndDuties1P

            , std.TaxOow1 
            , std.TaxOow2 
            , std.TaxOow3 
            , std.TaxOow4 
            , std.TaxOow5 
            , std.TaxOow1P
            
            , std.TaxAndDutiesW

            , ISNULL(case when @approved = 0 then r.Cost else r.Cost_approved end, 0) as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 
				   then 
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm, 
							fsc.TravelCost, 
							fsc.LabourCost, 
							fst.PerformanceRate, 
							std.ExchangeRate,
							fsc.TravelTime,
							fsc.repairTime,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor)
					else
						Hardware.CalcByFieldServicePerYear(
							fst.TimeAndMaterialShare_norm_Approved, 
							fsc.TravelCost_Approved, 
							fsc.LabourCost_Approved, 
							fst.PerformanceRate_Approved, 
							std.ExchangeRate,
							fsc.TravelTime_Approved,
							fsc.repairTime_Approved,
							std.OnsiteHourlyRates,
							UpliftFactor.OohUpliftFactor_Approved)

               end as FieldServicePerYear

            --##### SERVICE SUPPORT COST #########                                                                                               
            , case when dur.IsProlongation = 1 then std.ServiceSupportPerYearWithoutSar else std.ServiceSupportPerYear end as ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 
                   then lc.ExpressDelivery          +
                        lc.HighAvailabilityHandling +
                        lc.StandardDelivery         +
                        lc.StandardHandling         +
                        lc.ReturnDeliveryFactory    +
                        lc.TaxiCourierDelivery      
                   else lc.ExpressDelivery_Approved          +
                        lc.HighAvailabilityHandling_Approved +
                        lc.StandardDelivery_Approved         +
                        lc.StandardHandling_Approved         +
                        lc.ReturnDeliveryFactory_Approved    +
                        lc.TaxiCourierDelivery_Approved     
                end / std.ExchangeRate as LogisticPerYear

                                                                                                                       
            , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
                    else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
                end / std.ExchangeRate as MarkupOtherCost                      
            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
                    else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
                end as MarkupFactorOtherCost                

            --####### PROACTIVE COST ###################
            , case when proSla.Name = '0' 
                    then 0 --we don't calc proactive(none)
                    else std.LocalRemoteAccessSetup + dur.Value * (
                                      std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
                                    + std.LocalPreparation * proSla.LocalPreparationShcRepetition                      
                                    + std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
                                    + std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
                                    + std.Travel * proSla.TravellingTimeRepetition                                     
                                    + std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
                                )
                end as ProActive

            --We don't use STDW and credits for Prolongation
            , case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarranty       end as LocalServiceStandardWarranty
            , case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarrantyManual end as LocalServiceStandardWarrantyManual

            , std.Credit1 
            , std.Credit2 
            , std.Credit3 
            , std.Credit4 
            , std.Credit5 
            , case when dur.IsProlongation <> 1 then std.Credits end as Credits

            --########## MANUAL COSTS ################
            , man.ListPrice          / std.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / std.ExchangeRate as DealerPrice                 
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC      / std.ExchangeRate) end as ServiceTCManual                   
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP     / std.ExchangeRate) end as ReActiveTPManual                   
            , man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  

            , man.ReleaseDate                           as ReleaseDate
            , u2.Name                                   as ReleaseUserName
            , u2.Email                                  as ReleaseUserEmail

            , man.ChangeDate                            
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

    FROM Hardware.CalcStdw(@approved, @cnt, @wg) std 

    INNER JOIN Portfolio.GetBySlaFspPaging(@cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on std.CountryId = m.CountryId and std.WgId = m.WgId 

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.ReinsuranceCalc r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType
    LEFT JOIN Hardware.UpliftFactor ON UpliftFactor.Country = m.CountryId AND UpliftFactor.Wg = m.WgId AND UpliftFactor.[Availability] = m.AvailabilityId

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId
)");

            //SODA.GetHw
            this.repositorySet.ExecuteSql(@"
alter function SODA.GetHw()
returns @tbl table(
      Country             nvarchar(64)
    , CountryGroup        nvarchar(64)
    , Fsp                 nvarchar(64)
    , Sog                 nvarchar(64)
    , Wg                  nvarchar(64)
    , TP_Released         float
    , TP_Manual           float
    , ListPrice           float
    , DealerDiscount      float
    , DealerPrice         float
    , ProActive           float
    , ReleaseDate         datetime
    , UserName            nvarchar(64)
    , UserEmail           nvarchar(64)
)
as
begin

    declare @cnt table (
          Id bigint not null INDEX IX1 CLUSTERED
        , Name nvarchar(255)
        , ISO3CountryCode nvarchar(255)
        , CountryGroup nvarchar(255)
        , ExchangeRate float
        , CanOverrideTransferCostAndPrice bit
        , CanStoreListAndDealerPrices     bit
    );
    insert into @cnt
    select  c.Id
        , c.Name as Country
        , c.ISO3CountryCode
        , cg.Name
        , er.Value
        , c.CanOverrideTransferCostAndPrice
        , c.CanStoreListAndDealerPrices
    from InputAtoms.Country c 
    left join InputAtoms.CountryGroup cg on cg.id = c.CountryGroupId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId;

    declare @wg table (
          Id bigint not null INDEX IX1 CLUSTERED
        , Name nvarchar(8)
        , Sog  nvarchar(8)
    );
    insert into @wg
    select wg.Id, wg.Name, sog.Name
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.Id = wg.SogId
    where wg.Deactivated = 0 and wg.WgType = 1;

    insert into @tbl
    select 
              c.Name as Country
            , c.CountryGroup
      
            , fsp.Name as Fsp

            , wg.Sog
            , wg.Name as Wg

            , man.ServiceTP_Released / c.ExchangeRate   as TP_Released
            , case when c.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTP / c.ExchangeRate) end     as TP_Manual                   

            , case when CanStoreListAndDealerPrices = 1 then man.ListPrice          / c.ExchangeRate end    as ListPrice                   
            , case when CanStoreListAndDealerPrices = 1 then man.DealerDiscount                      end    as DealerDiscount              
            , case when CanStoreListAndDealerPrices = 1 then man.DealerPrice        / c.ExchangeRate end    as DealerPrice                 

            --####### PROACTIVE COST ###############################################################################################################################
            , case when proSla.Name = '0' 
                    then 0 --we don't calc proactive(none)
                    else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved + dur.Value * (

                                  pro.LocalRegularUpdateReadyEffort_Approved        * proSla.LocalRegularUpdateReadyRepetition        * pro.OnSiteHourlyRate_Approved       
                                + pro.LocalPreparationShcEffort_Approved            * proSla.LocalPreparationShcRepetition            * pro.OnSiteHourlyRate_Approved         
                                + pro.LocalRemoteShcCustomerBriefingEffort_Approved * proSla.LocalRemoteShcCustomerBriefingRepetition * pro.OnSiteHourlyRate_Approved
                                + pro.LocalOnSiteShcCustomerBriefingEffort_Approved * proSla.LocalOnsiteShcCustomerBriefingRepetition * pro.OnSiteHourlyRate_Approved
                                + pro.TravellingTime_Approved                       * proSla.TravellingTimeRepetition                 * pro.OnSiteHourlyRate_Approved                   
                                + pro.CentralExecutionShcReportCost_Approved        * proSla.CentralExecutionShcReportRepetition          

                            ) 
                end as ProActive
            --######################################################################################################################################################
            , man.ReleaseDate                           as ReleaseDate
            , u.Name                                    as UserName
            , u.Email                                   as UserEmail

    from Hardware.ManualCost man
    join Portfolio.LocalPortfolio p on p.Id = man.PortfolioId
    join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = p.SlaHash and p.Sla = p.Sla

    join @cnt c on c.id = p.CountryId
    join @wg wg on wg.id = p.WgId
    join Dependencies.Duration dur on dur.id = p.DurationId
    join Dependencies.ProActiveSla proSla on proSla.id = p.ProactiveSlaId

    left join Hardware.ProActive pro ON pro.Country= p.CountryId and pro.Wg = p.WgId

    left join dbo.[User] u on u.Id = man.ChangeUserId

    where man.ServiceTP_Released is not null;

    return;

end
");

            //[Archive].[spGetProActive]
            this.repositorySet.ExecuteSql(@"
alter procedure [Archive].[spGetProActive]
AS
begin
    select  c.Name as Country
          , c.Region
          , c.ClusterRegion

          , wg.Name as Wg
          , wg.Description as WgDescription
          , wg.Pla
          , wg.Sog

          , ccg.Name                             as ContractGroup
          , ccg.Code                             as ContractGroupCode

          , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteAccessSetupPreparationEffort
          , pro.LocalRegularUpdateReadyEffort_Approved           as LocalRegularUpdateReadyEffort
          , pro.LocalPreparationShcEffort_Approved               as LocalPreparationShcEffort
          , pro.CentralExecutionShcReportCost_Approved           as CentralExecutionShcReportCost
          , pro.LocalRemoteShcCustomerBriefingEffort_Approved    as LocalRemoteShcCustomerBriefingEffort
          , pro.LocalOnSiteShcCustomerBriefingEffort_Approved    as LocalOnSiteShcCustomerBriefingEffort
          , pro.TravellingTime_Approved                          as TravellingTime
          , pro.OnSiteHourlyRate_Approved                        as OnSiteHourlyRate

    from Hardware.ProActive pro
    join Archive.GetCountries() c on c.id = pro.Country
    join Archive.GetWg(null) wg on wg.id = pro.Wg
    join InputAtoms.CentralContractGroup ccg on ccg.Id = pro.CentralContractGroup
    order by c.Name, wg.Name
end");
        }

        private QueryColumnInfo[] BuildNormColumns(CostBlockEntityMeta costBlock, string costElementId)
        {
            var costElementField = costBlock.CostElementsFields[costElementId];
            var costElementApprovedField = costBlock.CostElementsApprovedFields[costElementField];

            return new[]
            {
                BuildNormColumn($"{costElementId}_norm", costElementField),
                BuildNormColumn($"{costElementId}_norm_Approved", costElementApprovedField)
            };

            QueryColumnInfo BuildNormColumn(string name, FieldMeta field)
            {
                return new QueryColumnInfo
                {
                    Alias = name,
                    Query = SqlOperators.Devide(new ColumnSqlBuilder(field.Name), new ValueSqlBuilder(100)).ToSqlBuilder()
                };
            }
        }
    }
}
