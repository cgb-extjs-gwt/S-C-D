using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2020_02_13_09_07 : IMigrationAction
    {
        private readonly IDataMigrator dataMigrator;
        private readonly IRepositorySet repositorySet;
        private readonly DomainEnitiesMeta meta;

        public int Number => 150;

        public string Description => "Add cost element 'OohUpliftFactor' in 'FieldServiceCost' cost block";

        public Migration_2020_02_13_09_07(
            IDataMigrator dataMigrator,
            IRepositorySet repositorySet, 
            DomainEnitiesMeta meta)
        {
            this.dataMigrator = dataMigrator;
            this.repositorySet = repositorySet;
            this.meta = meta;
        }

        public void Execute()
        {
            const string UpliftFactorCostElement = "OohUpliftFactor";

            this.dataMigrator.AddCostElements(new[]
            {
                new CostElementInfo
                {
                    Meta = this.meta.CostBlocks[MetaConstants.HardwareSchema, "FieldServiceCost"],
                    CostElementIds = new[] { UpliftFactorCostElement }
                }
            });

            //ALTER TABLE [Hardware].[FieldServiceCalc] 
            this.repositorySet.ExecuteSql($@"
ALTER TABLE [Hardware].[FieldServiceCalc] 
ADD 
    [{UpliftFactorCostElement}] FLOAT NULL,
    [{UpliftFactorCostElement}_Approved] FLOAT NULL");

            //TRIGGER [Hardware].[FieldServiceCost_Inserted]
            this.repositorySet.ExecuteSql(@"
ALTER TRIGGER [Hardware].[FieldServiceCost_Inserted]
ON [Hardware].[FieldServiceCost]
After INSERT
AS BEGIN

    insert into Hardware.FieldServiceCalc(
                Country
              , Wg
              , ServiceLocation
              , RepairTime
              , RepairTime_Approved
              , TravelTime
              , TravelTime_Approved
              , TravelCost
              , TravelCost_Approved
              , LabourCost
              , LabourCost_Approved
			  , OohUpliftFactor
			  , OohUpliftFactor_Approved)
    select    i.Country
            , i.Wg
            , i.ServiceLocation
            , MIN(i.RepairTime)
            , MIN(i.RepairTime_Approved)
            , MIN(i.TravelTime)
            , MIN(i.TravelTime_Approved)
            , MIN(i.TravelCost)
            , MIN(i.TravelCost_Approved)
            , MIN(i.LabourCost)
            , MIN(i.LabourCost_Approved)
			, MIN(i.OohUpliftFactor)
			, MIN(i.OohUpliftFactor_Approved)
    from inserted i
    left join Hardware.FieldServiceCalc fsc on fsc.Country = i.Country and fsc.Wg = i.Wg and fsc.ServiceLocation = i.ServiceLocation
    where fsc.Country is null
    group by i.Country, i.Wg, i.ServiceLocation

    insert into Hardware.FieldServiceTimeCalc(
                  Country
                , Wg
                , ReactionTimeType
                , PerformanceRate
                , PerformanceRate_Approved
                , TimeAndMaterialShare
                , TimeAndMaterialShare_Approved)
    select    i.Country
            , i.Wg
            , i.ReactionTimeType
            , MIN(i.PerformanceRate)
            , MIN(i.PerformanceRate_Approved)
            , MIN(i.TimeAndMaterialShare)
            , MIN(i.TimeAndMaterialShare_Approved)
    from inserted i
    join Dependencies.ReactionTimeType rtt on rtt.id = i.ReactionTimeType and rtt.IsDisabled = 0
    left join Hardware.FieldServiceTimeCalc fsc on fsc.Country = i.Country and fsc.Wg = i.Wg and fsc.ReactionTimeType = i.ReactionTimeType
    where fsc.Country is null
    group by i.Country, i.Wg, i.ReactionTimeType;
END");

            //TRIGGER [Hardware].[FieldServiceCost_Updated]
            this.repositorySet.ExecuteSql(@"
ALTER TRIGGER [Hardware].[FieldServiceCost_Updated]
ON [Hardware].[FieldServiceCost]
After UPDATE
AS BEGIN

    update fsc 
            set RepairTime = c.RepairTime
              , RepairTime_Approved = c.RepairTime_Approved

              , TravelTime = c.TravelTime
              , TravelTime_Approved = c.TravelTime_Approved
              
              , TravelCost = c.TravelCost
              , TravelCost_Approved = c.TravelCost_Approved
              
              , LabourCost = c.LabourCost
              , LabourCost_Approved = c.LabourCost_Approved

			  , OohUpliftFactor = c.OohUpliftFactor
			  , OohUpliftFactor_Approved = c.OohUpliftFactor_Approved
    from Hardware.FieldServiceCalc fsc
    join inserted c on c.Country = fsc.Country and c.Wg = fsc.Wg and c.ServiceLocation = fsc.ServiceLocation and c.DeactivatedDateTime is null;

    update fst 
            set PerformanceRate = c.PerformanceRate
              , PerformanceRate_Approved = c.PerformanceRate_Approved

              , TimeAndMaterialShare = c.TimeAndMaterialShare
              , TimeAndMaterialShare_Approved = c.TimeAndMaterialShare_Approved
    from Hardware.FieldServiceTimeCalc fst
    join inserted c on c.Country = fst.Country and c.Wg = fst.Wg and c.ReactionTimeType = fst.ReactionTimeType  and c.DeactivatedDateTime is null

END");

            // FUNCTION [Hardware].[GetCalcMember2]
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

                   then (1 - fst.TimeAndMaterialShare_norm) * (fsc.TravelCost + fsc.LabourCost + fst.PerformanceRate) / std.ExchangeRate + 
                            fst.TimeAndMaterialShare_norm * ((fsc.TravelTime + fsc.repairTime) * std.OnsiteHourlyRates + fst.PerformanceRate / std.ExchangeRate) 

                   else (1 - fst.TimeAndMaterialShare_norm_Approved) * (fsc.TravelCost_Approved + fsc.LabourCost_Approved + fst.PerformanceRate_Approved) / std.ExchangeRate + 
                            fst.TimeAndMaterialShare_norm_Approved * ((fsc.TravelTime_Approved + fsc.repairTime_Approved) * std.OnsiteHourlyRates + fst.PerformanceRate_Approved / std.ExchangeRate) 

               end as FieldServicePerYear

            , case when @approved = 0 
                   then isnull(fsc.OohUpliftFactor, 0)
                   else isnull(fsc.OohUpliftFactor_Approved, 0)
              end as OohUpliftFactor
   
            --##### SERVICE SUPPORT COST #########                                                                                               
            , std.ServiceSupportPerYear

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

            , std.LocalServiceStandardWarranty
            , std.LocalServiceStandardWarrantyManual
            , std.Credit1
            , std.Credit2
            , std.Credit3
            , std.Credit4
            , std.Credit5
            , std.Credits

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

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId
)");

            //FUNCTION [Hardware].[GetCosts2]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[GetCosts2](
    @approved                bit,
    @cnt dbo.ListID          readonly,
    @fsp                     nvarchar(255),
    @hasFsp                  bit,
    @wg dbo.ListID           readonly,
    @av dbo.ListID           readonly,
    @dur dbo.ListID          readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID          readonly,
    @pro dbo.ListID          readonly,
    @lastid                  bigint,
    @limit                   int
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , ISNULL(m.AvailabilityFee, 0) as AvailabilityFeeOrZero
       
        from Hardware.GetCalcMember2(@approved, @cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

                , isnull(case when m.DurationId = 1 then m.Reinsurance end, 0) as Reinsurance1
                , isnull(case when m.DurationId = 2 then m.Reinsurance end, 0) as Reinsurance2
                , isnull(case when m.DurationId = 3 then m.Reinsurance end, 0) as Reinsurance3
                , isnull(case when m.DurationId = 4 then m.Reinsurance end, 0) as Reinsurance4
                , isnull(case when m.DurationId = 5 then m.Reinsurance end, 0) as Reinsurance5
                , isnull(case when m.DurationId = 6 then m.Reinsurance end, 0) as Reinsurance1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

        from CostCte2 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1 + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2 + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3 + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4 + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5 + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect1P + m.AvailabilityFeeOrZero             as ServiceTP1P

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

                , m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
                , m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
                , m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
                , m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
                , m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
                , m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

        from CostCte5 m
    )
    , CostCte7 as (
        select m.rownum
             , m.Id

             --SLA

             , m.Fsp
             , m.CountryId
             , m.Country
             , m.CurrencyId
             , m.Currency
             , m.ExchangeRate
             , m.SogId
             , m.Sog
             , m.WgId
             , m.Wg
             , m.AvailabilityId
             , m.Availability
             , m.DurationId
             , m.Duration
             , m.Year
             , m.IsProlongation
             , m.ReactionTimeId
             , m.ReactionTime
             , m.ReactionTypeId
             , m.ReactionType
             , m.ServiceLocationId
             , m.ServiceLocation
             , m.ProActiveSlaId

             , m.ProActiveSla

             , m.Sla
             , m.SlaHash

             , m.StdWarranty
             , m.StdWarrantyLocation

             --Cost

             , m.AvailabilityFee * m.Year as AvailabilityFee
             , m.TaxAndDutiesW
             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow


             , m.Reinsurance
             , m.ProActive
             , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

             , m.MaterialW
             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow

             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) * (1 + m.OohUpliftFactor/100) as FieldServiceCost
             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
       
             , m.LocalServiceStandardWarranty
             , m.LocalServiceStandardWarrantyManual
       
             , m.Credits

             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
             , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

             , m.ServiceTC1
             , m.ServiceTC2
             , m.ServiceTC3
             , m.ServiceTC4
             , m.ServiceTC5
             , m.ServiceTC1P

             , m.ServiceTP1
             , m.ServiceTP2
             , m.ServiceTP3
             , m.ServiceTP4
             , m.ServiceTP5
             , m.ServiceTP1P

             , m.ListPrice
             , m.DealerDiscount
             , m.DealerPrice
             , m.ServiceTCManual
             , m.ServiceTPManual
             , coalesce(m.ServiceTCManual, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P)) as ServiceTCResult
             , coalesce(m.ServiceTPManual, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P)) as ServiceTPResult
             , m.ServiceTP_Released

             , m.ReleaseDate
             , m.ReleaseUserName
             , m.ReleaseUserEmail

             , m.ChangeDate
             , m.ChangeUserName
             , m.ChangeUserEmail

           from CostCte6 m
    )
    select m.*
         
         , m.ServiceTC as ReActiveTC
         , m.ServiceTP as ReActiveTP

         , m.ServiceTC + coalesce(m.ProActive, 0) as FullServiceTC
         , m.ServiceTP + coalesce(m.ProActive, 0) as FullServiceTP

    from CostCte7 m
)");

            //FUNCTION [Hardware].[GetCalcMember]
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

                   then (1 - fst.TimeAndMaterialShare_norm) * (fsc.TravelCost + fsc.LabourCost + fst.PerformanceRate) / std.ExchangeRate + 
                            fst.TimeAndMaterialShare_norm * ((fsc.TravelTime + fsc.repairTime) * std.OnsiteHourlyRates + fst.PerformanceRate / std.ExchangeRate) 

                   else (1 - fst.TimeAndMaterialShare_norm_Approved) * (fsc.TravelCost_Approved + fsc.LabourCost_Approved + fst.PerformanceRate_Approved) / std.ExchangeRate + 
                            fst.TimeAndMaterialShare_norm_Approved * ((fsc.TravelTime_Approved + fsc.repairTime_Approved) * std.OnsiteHourlyRates + fst.PerformanceRate_Approved / std.ExchangeRate) 

               end as FieldServicePerYear

            , case when @approved = 0 
                then isnull(fsc.OohUpliftFactor, 0)
                else isnull(fsc.OohUpliftFactor_Approved, 0)
              end as OohUpliftFactor

            --##### SERVICE SUPPORT COST #########                                                                                               
            , std.ServiceSupportPerYear

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

            , std.LocalServiceStandardWarranty
            , std.LocalServiceStandardWarrantyManual
            , std.Credit1
            , std.Credit2
            , std.Credit3
            , std.Credit4
            , std.Credit5
            , std.Credits

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

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId
)");

            //FUNCTION [Hardware].[GetCosts]
            this.repositorySet.ExecuteSql(@"
ALTER FUNCTION [Hardware].[GetCosts](
    @approved bit,
    @cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , ISNULL(m.AvailabilityFee, 0) as AvailabilityFeeOrZero

        from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

                , isnull(case when m.DurationId = 1 then m.Reinsurance end, 0) as Reinsurance1
                , isnull(case when m.DurationId = 2 then m.Reinsurance end, 0) as Reinsurance2
                , isnull(case when m.DurationId = 3 then m.Reinsurance end, 0) as Reinsurance3
                , isnull(case when m.DurationId = 4 then m.Reinsurance end, 0) as Reinsurance4
                , isnull(case when m.DurationId = 5 then m.Reinsurance end, 0) as Reinsurance5
                , isnull(case when m.DurationId = 6 then m.Reinsurance end, 0) as Reinsurance1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

        from CostCte2 m
    )
    , CostCte5 as (
        select m.*

                , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1 + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
                , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2 + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
                , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3 + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
                , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4 + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
                , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5 + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
                , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect1P + m.AvailabilityFeeOrZero             as ServiceTP1P

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

                , m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
                , m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
                , m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
                , m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
                , m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
                , m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

        from CostCte5 m
    )    
    select m.rownum
            , m.Id

            --SLA

            , m.Fsp
            , m.CountryId
            , m.Country
            , m.CurrencyId
            , m.Currency
            , m.ExchangeRate
            , m.SogId
            , m.Sog
            , m.WgId
            , m.Wg
            , m.AvailabilityId
            , m.Availability
            , m.DurationId
            , m.Duration
            , m.Year
            , m.IsProlongation
            , m.ReactionTimeId
            , m.ReactionTime
            , m.ReactionTypeId
            , m.ReactionType
            , m.ServiceLocationId
            , m.ServiceLocation
            , m.ProActiveSlaId

            , m.ProActiveSla

            , m.Sla
            , m.SlaHash

            , m.StdWarranty
            , m.StdWarrantyLocation

            --Cost

            , m.AvailabilityFee * m.Year as AvailabilityFee
            , m.TaxAndDutiesW
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow


            , m.Reinsurance
            , m.ProActive
            , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

            , m.MaterialW
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow

            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) * (1 + m.OohUpliftFactor/100) as FieldServiceCost
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect

            , m.LocalServiceStandardWarranty
            , m.LocalServiceStandardWarrantyManual

            , m.Credits

            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
            , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

            , m.ServiceTC1
            , m.ServiceTC2
            , m.ServiceTC3
            , m.ServiceTC4
            , m.ServiceTC5
            , m.ServiceTC1P

            , m.ServiceTP1
            , m.ServiceTP2
            , m.ServiceTP3
            , m.ServiceTP4
            , m.ServiceTP5
            , m.ServiceTP1P

            , m.ListPrice
            , m.DealerDiscount
            , m.DealerPrice
            , m.ServiceTCManual
            , m.ServiceTPManual
            , coalesce(m.ServiceTCManual, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P)) as ServiceTCResult
            , coalesce(m.ServiceTPManual, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P)) as ServiceTPResult
            , m.ServiceTP_Released

            , m.ReleaseDate
            , m.ReleaseUserName
            , m.ReleaseUserEmail

            , m.ChangeDate
            , m.ChangeUserName
            , m.ChangeUserEmail

        from CostCte6 m
)");
        }
    }
}
