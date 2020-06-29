USE [SCD_2]

IF OBJECT_ID('[Hardware].[GetCalcMemberYear]') IS NOT NULL
  DROP FUNCTION [Hardware].[GetCalcMemberYear];
go 

CREATE FUNCTION [Hardware].[GetCalcMemberYear] (
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
    @limit          int,
	@projectId  BIGINT = NULL
)
RETURNS TABLE 
AS
RETURN 
(
	WITH IsProjCalc AS 
	(
		SELECT CASE WHEN @projectId IS NULL THEN 0 ELSE 1 END AS IsProjCalc 
	),
	ProjCalc AS 
	(
		SELECT 
			std.*,
			m.ProActiveSlaId,
			m.rownum,
			m.Fsp,
			m.Sla,
            m.SlaHash,

			case when @projectId IS NOT NULL then ProjectItem.Id else m.Id end as Id,
			case when @projectId IS NOT NULL then ProjectItem.ReactionTypeId else m.ReactionTypeId end as ReactionTypeId,
			case when @projectId IS NOT NULL then ProjectItem.ServiceLocationId else m.ServiceLocationId end as ServiceLocationId,
			case when @projectId IS NULL then m.AvailabilityId end as AvailabilityId,
			case when @projectId IS NOT NULL then ProjectItem.Availability_Name else av.[Name] end as [Availability],
			case when @projectId IS NULL then m.DurationId end as DurationId,
			case when @projectId IS NOT NULL then 0 else dur.IsProlongation end as DurationIsProlongation,
			case when @projectId IS NOT NULL then ProjectItem.Duration_Name else dur.[Name] end as Duration,
			case when @projectId IS NOT NULL then ProjectItem.Duration_Months else dur.[Value] * 12 end as DurationMonths,
			case when @projectId IS NULL then m.ReactionTimeId end as ReactionTimeId,
			case when @projectId IS NOT NULL then ProjectItem.ReactionTime_Name else rtime.[Name] end as ReactionTime,
			case when @projectId IS NOT NULL
				 then ProjectItem.Reinsurance_Flatfee * ISNULL(1 + Reinsurance_UpliftFactor / 100, 1) / ExchangeRate.[Value]
				 else case when @approved = 0 then r.Cost else r.Cost_approved end
			end as Cost,
			case when @projectId IS NOT NULL or afEx.id is not null then std.Fee else 0 end as AvailabilityFee,

			case when @approved = 0 then fsw.RepairTime else fsw.RepairTime_Approved end as RepairTime,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_LabourCost, fsl.LabourCost, fsl.LabourCost_Approved) AS LabourCost,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_TravelCost, fsl.TravelCost, fsl.TravelCost_Approved) AS TravelCost,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_TravelTime, fsl.TravelTime, fsl.TravelTime_Approved) AS TravelTime,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_PerformanceRate, fst.PerformanceRate, fst.PerformanceRate_Approved) AS PerformanceRate,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_TimeAndMaterialShare, fst.TimeAndMaterialShare, fst.TimeAndMaterialShare_Approved) / 100 AS TimeAndMaterialShare_norm,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.FieldServiceCost_OohUpliftFactor, fsa.OohUpliftFactor, fsa.OohUpliftFactor_Approved) AS OohUpliftFactor,

			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_StandardHandling, lc.StandardHandling, lc.StandardHandling_Approved) AS StandardHandling,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_HighAvailabilityHandling, lc.HighAvailabilityHandling, lc.HighAvailabilityHandling_Approved) AS HighAvailabilityHandling,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_StandardDelivery, lc.StandardDelivery, lc.StandardDelivery_Approved) AS StandardDelivery,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_ExpressDelivery, lc.ExpressDelivery, lc.ExpressDelivery_Approved) AS ExpressDelivery,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_TaxiCourierDelivery, lc.TaxiCourierDelivery, lc.TaxiCourierDelivery_Approved) AS TaxiCourierDelivery,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.LogisticsCosts_ReturnDeliveryFactory, lc.ReturnDeliveryFactory, lc.ReturnDeliveryFactory_Approved) AS ReturnDeliveryFactory,

			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_Markup, moc.Markup, moc.Markup_Approved) AS Markup,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_MarkupFactor, moc.MarkupFactor, moc.MarkupFactor_Approved) / 100 AS MarkupFactor,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_ProlongationMarkup, moc.ProlongationMarkup, moc.ProlongationMarkup_Approved) AS ProlongationMarkup,
			Hardware.CalcByProjectFlag(IsProjCalc, @approved, ProjectItem.MarkupOtherCosts_ProlongationMarkupFactor, moc.ProlongationMarkupFactor, moc.ProlongationMarkupFactor_Approved) / 100 AS ProlongationMarkupFactor

		FROM Hardware.CalcStdwYear(@approved, @cnt, @wg, @projectId) std 

		CROSS JOIN IsProjCalc

		LEFT JOIN Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on @projectId IS NULL and std.CountryId = m.CountryId and std.WgId = m.WgId

		LEFT JOIN Dependencies.Availability av on @projectId IS NULL and av.Id = m.AvailabilityId

		LEFT JOIN Dependencies.Duration dur on @projectId IS NULL and dur.id = m.DurationId

		LEFT JOIN Dependencies.ReactionTime rtime on @projectId IS NULL and rtime.Id = m.ReactionTimeId

		LEFT JOIN Admin.AvailabilityFee afEx on @projectId IS NULL and afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

		LEFT JOIN ProjectCalculator.ProjectItem ON ProjectItem.ProjectId = @projectId AND ProjectItem.CountryId = std.CountryId AND ProjectItem.WgId = std.WgId

		LEFT JOIN Hardware.ReinsuranceCalc r on @projectId IS NULL AND r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

		LEFT JOIN Hardware.FieldServiceWg fsw on fsw.Wg = std.WgId and fsw.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceLocation fsl on @projectId IS NULL and fsl.Country = m.CountryId and fsl.Wg = m.WgId and fsl.ServiceLocation = m.ServiceLocationId and fsl.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceReactionTimeType fst on @projectId IS NULL and fst.Country = m.CountryId and fst.Wg = m.WgId and fst.ReactionTimeType = m.ReactionTime_ReactionType and fst.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceAvailability fsa on @projectId IS NULL and fsa.Country = m.CountryId and fsa.Wg = m.WgId and fsa.[Availability] = m.AvailabilityId and fsa.DeactivatedDateTime is null

		LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

		LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

		LEFT JOIN [References].ExchangeRate on @projectId IS NOT NULL and ProjectItem.Reinsurance_CurrencyId = ExchangeRate.CurrencyId
	)
    SELECT    m.rownum
            , m.Id

            --SLA

            , m.Fsp
            , m.CountryId          
            , m.Country
            , m.CurrencyId
            , m.Currency
            , m.ExchangeRate
            , m.WgId
            , m.Wg
            , m.SogId
            , m.Sog
            , m.DurationId
			, m.Duration
			, m.DurationMonths / 12 as Year
			, m.DurationMonths
			, m.DurationIsProlongation as IsProlongation
			, m.Months			 as StdMonths
			, m.IsProlongation   as StdIsProlongation
            , m.AvailabilityId
			, m.Availability
            , m.ReactionTimeId
			, m.ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

			, m.StdWarrantyMonths / 12 as StdWarranty
			, m.StdWarrantyMonths
            , m.StdWarrantyLocation

            --Cost values

			, m.AFR

			, m.MatCost
			, m.MatW

			, m.MatOow

			, m.TaxW
			, m.TaxAndDuties

			, m.TaxOow
            
			, ISNULL(m.Cost, 0) as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
			, Hardware.CalcByFieldServicePerYear(
					m.TimeAndMaterialShare_norm, 
					m.TravelCost, 
					m.LabourCost, 
					m.PerformanceRate, 
					m.ExchangeRate,
					m.TravelTime,
					m.repairTime,
					m.OnsiteHourlyRates,
					m.OohUpliftFactor) as FieldServicePerYear

            --##### SERVICE SUPPORT COST #########                                                                                               
			, case when m.DurationIsProlongation = 1 then m.ServiceSupportPerYearWithoutSar else m.ServiceSupportPerYear end as ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
			, (m.ExpressDelivery +
               m.HighAvailabilityHandling +
               m.StandardDelivery         +
               m.StandardHandling         +
               m.ReturnDeliveryFactory    +
               m.TaxiCourierDelivery) / m.ExchangeRate as LogisticPerYear

                                                                                                                       
			, m.AvailabilityFee

			, case when m.DurationIsProlongation = 0 then m.Markup else m.ProlongationMarkup end / m.ExchangeRate as MarkupOtherCost     
			, case when m.DurationIsProlongation = 0 then m.MarkupFactor else m.ProlongationMarkupFactor end as MarkupFactorOtherCost     

            --####### PROACTIVE COST ###################
			, case when proSla.Name = '0' 
                    then 0 --we don't calc proactive(none)
                    else m.LocalRemoteAccessSetup + m.DurationMonths * (
                                      m.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
                                    + m.LocalPreparation * proSla.LocalPreparationShcRepetition                      
                                    + m.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
                                    + m.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
                                    + m.Travel * proSla.TravellingTimeRepetition                                     
                                    + m.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
                                )
                end as ProActive

            --We don't use STDW and credits for Prolongation
			, case when m.DurationIsProlongation <> 1 then m.LocalServiceStandardWarranty       end as LocalServiceStandardWarranty
            , case when m.DurationIsProlongation <> 1 then m.LocalServiceStandardWarrantyManual end as LocalServiceStandardWarrantyManual
			, case when m.DurationIsProlongation <> 1 then Hardware.AddMarkup(m.LocalServiceStandardWarranty, m.RiskFactorStandardWarranty, m.RiskStandardWarranty) end as LocalServiceStandardWarrantyWithRisk

			, case when m.DurationIsProlongation <> 1 then m.Credit end as Credit

            --########## MANUAL COSTS ################
			, man.ListPrice          / m.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / m.ExchangeRate as DealerPrice                 
            , case when m.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / m.ExchangeRate) end as ServiceTCManual                   
            , case when m.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP    / m.ExchangeRate) end as ReActiveTPManual                   
            , man.ServiceTP_Released / m.ExchangeRate as ServiceTP_Released                  

            , man.ReleaseDate                           as ReleaseDate
            , u2.Name                                   as ReleaseUserName
            , u2.Email                                  as ReleaseUserEmail

            , man.ChangeDate                            
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

			, m.LabourCost
			, m.TravelCost
			, m.PerformanceRate
			, m.TravelTime
			, m.RepairTime
			, m.OnsiteHourlyRates
			, m.TimeAndMaterialShare_norm
			, m.OohUpliftFactor
			, m.TaxW AS TaxAndDutiesW
			, m.MarkupFactorStandardWarranty
			, m.MarkupStandardWarranty
			, m.RiskFactorStandardWarranty
			, m.RiskStandardWarranty
			, m.[1stLevelSupportCosts]
			, m.[2ndLevelSupportCosts]
			, m.[Sar]
		    , m.[MaterialCostWarranty]
			, m.[MaterialCostOow]
			, m.[StandardHandling]
			, m.[HighAvailabilityHandling]
			, m.[StandardDelivery]
			, m.[ExpressDelivery]
			, m.[TaxiCourierDelivery]
			, m.[ReturnDeliveryFactory]

    FROM ProjCalc m

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    LEFT JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.ManualCost man on @projectId IS NULL and man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId

	WHERE m.Months <= m.DurationMonths and m.IsProlongation = m.DurationIsProlongation
)
go