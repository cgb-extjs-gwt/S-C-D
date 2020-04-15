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

            this.FieldServiceCostSplit(oldMeta, newMeta);
            this.CalculationUpdate();
        }

        private void FieldServiceCostSplit(DomainEnitiesMeta oldMeta, DomainEnitiesMeta newMeta)
        {
            const string FieldServiceCost = "FieldServiceCost";
            const string TimeAndMaterialShare = "TimeAndMaterialShare";
            const string FieldServiceTimeCalc = "FieldServiceTimeCalc";
            const string FieldServiceCalc = "FieldServiceCalc";
            const string UpliftFactor = "UpliftFactor";

            var oldCostBlock = oldMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost];

            AddAvailabilityColumn();

            var newCostBlocks = newMeta.CostBlocks.GetSome(MetaConstants.HardwareSchema, FieldServiceCost);

            this.dataMigrator.SplitCostBlock(oldCostBlock, newCostBlocks, newMeta);

            this.costBlockRepository.UpdateByCoordinates(newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "OohUpliftFactor"]);

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
                    newMeta.CostBlocks[MetaConstants.HardwareSchema, FieldServiceCost, "OohUpliftFactor"],
                },
                ignoreCoordinates: ignoreCoordinates);

            this.dataMigrator.DropCostBlock(oldCostBlock);

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
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTP     / std.ExchangeRate) end as ServiceTPManual                   
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
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / std.ExchangeRate) end as ServiceTCManual                   
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTP     / std.ExchangeRate) end as ServiceTPManual                   
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
