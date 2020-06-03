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
	@isProjectCalculator bit = 0
)
RETURNS TABLE 
AS
RETURN 
(
	WITH ProjCalc AS 
	(
		SELECT 
			std.*,
			--m.*,
			--m.Id as PortfolioId,
			m.ProActiveSlaId,
			m.rownum,
			m.Fsp,
			m.Sla,
            m.SlaHash,

			--case when @isProjectCalculator = 1 then Project.CountryId else m.CountryId end as CountryId,
			--case when @isProjectCalculator = 1 then Project.WgId else m.WgId end as WgId,
			
			case when @isProjectCalculator = 1 then Project.Id else m.Id end as Id,
			case when @isProjectCalculator = 1 then Project.ReactionTypeId else m.ReactionTypeId end as ReactionTypeId,
			case when @isProjectCalculator = 1 then Project.ServiceLocationId else m.ServiceLocationId end as ServiceLocationId,
			case when @isProjectCalculator = 0 then m.AvailabilityId end as AvailabilityId,
			case when @isProjectCalculator = 1 then Project.Availability_Name else av.[Name] end as [Availability],
			case when @isProjectCalculator = 0 then m.DurationId end as DurationId,
			case when @isProjectCalculator = 1 then 0 else dur.IsProlongation end as DurationIsProlongation,
			case when @isProjectCalculator = 1 then Project.Duration_Name else dur.[Name] end as Duration,
			case when @isProjectCalculator = 1 then Project.Duration_Months else dur.[Value] * 12 end as DurationMonths,
			case when @isProjectCalculator = 0 then m.ReactionTimeId end as ReactionTimeId,
			case when @isProjectCalculator = 1 then Project.ReactionTime_Name else rtime.[Name] end as ReactionTime,
			case when @isProjectCalculator = 1
				 then Project.Reinsurance_Flatfee * ISNULL(1 + Reinsurance_UpliftFactor / 100, 1) / ExchangeRate.[Value]
				 else case when @approved = 0 then r.Cost else r.Cost_approved end
			end as Cost,
			case when @isProjectCalculator = 1 or afEx.id is not null then std.Fee else 0 end as AvailabilityFee,

			case when @approved = 0 then fsw.RepairTime else fsw.RepairTime_Approved end as RepairTime,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_LabourCost, fsl.LabourCost, fsl.LabourCost_Approved) AS LabourCost,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_TravelCost, fsl.TravelCost, fsl.TravelCost_Approved) AS TravelCost,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_TravelTime, fsl.TravelTime, fsl.TravelTime_Approved) AS TravelTime,
			--Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_RepairTime, fsc.RepairTime, fsc.RepairTime_Approved) AS RepairTime,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_PerformanceRate, fst.PerformanceRate, fst.PerformanceRate_Approved) AS PerformanceRate,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_TimeAndMaterialShare, fst.TimeAndMaterialShare, fst.TimeAndMaterialShare_Approved) / 100 AS TimeAndMaterialShare_norm,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.FieldServiceCost_OohUpliftFactor, fsa.OohUpliftFactor, fsa.OohUpliftFactor_Approved) AS OohUpliftFactor,

			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_StandardHandling, lc.StandardHandling, lc.StandardHandling_Approved) AS StandardHandling,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_HighAvailabilityHandling, lc.HighAvailabilityHandling, lc.HighAvailabilityHandling_Approved) AS HighAvailabilityHandling,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_StandardDelivery, lc.StandardDelivery, lc.StandardDelivery_Approved) AS StandardDelivery,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_ExpressDelivery, lc.ExpressDelivery, lc.ExpressDelivery_Approved) AS ExpressDelivery,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_TaxiCourierDelivery, lc.TaxiCourierDelivery, lc.TaxiCourierDelivery_Approved) AS TaxiCourierDelivery,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.LogisticsCosts_ReturnDeliveryFactory, lc.ReturnDeliveryFactory, lc.ReturnDeliveryFactory_Approved) AS ReturnDeliveryFactory,

			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.MarkupOtherCosts_Markup, moc.Markup, moc.Markup_Approved) AS Markup,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.MarkupOtherCosts_MarkupFactor, moc.MarkupFactor, moc.MarkupFactor_Approved) / 100 AS MarkupFactor,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.MarkupOtherCosts_ProlongationMarkup, moc.ProlongationMarkup, moc.ProlongationMarkup_Approved) AS ProlongationMarkup,
			Hardware.CalcByProjectFlag(@isProjectCalculator, @approved, Project.MarkupOtherCosts_ProlongationMarkupFactor, moc.ProlongationMarkupFactor, moc.ProlongationMarkupFactor_Approved) / 100 AS ProlongationMarkupFactor

		FROM Hardware.CalcStdwYear(@approved, @cnt, @wg, @isProjectCalculator) std 

		LEFT JOIN Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m on @isProjectCalculator = 0 and std.CountryId = m.CountryId and std.WgId = m.WgId

		LEFT JOIN Dependencies.Availability av on @isProjectCalculator = 0 and av.Id = m.AvailabilityId

		LEFT JOIN Dependencies.Duration dur on @isProjectCalculator = 0 and dur.id = m.DurationId

		LEFT JOIN Dependencies.ReactionTime rtime on @isProjectCalculator = 0 and rtime.Id = m.ReactionTimeId

		LEFT JOIN Admin.AvailabilityFee afEx on @isProjectCalculator = 0 and afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

		LEFT JOIN ProjectCalculator.Project ON @isProjectCalculator = 1 AND Project.CountryId = std.CountryId AND Project.WgId = std.WgId

		LEFT JOIN Hardware.ReinsuranceCalc r on @isProjectCalculator = 0 AND r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

		--LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
		--LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType
		--LEFT JOIN Hardware.UpliftFactor ON UpliftFactor.Country = m.CountryId AND UpliftFactor.Wg = m.WgId AND UpliftFactor.[Availability] = m.AvailabilityId

		LEFT JOIN Hardware.FieldServiceWg fsw on fsw.Wg = std.WgId and fsw.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceLocation fsl on @isProjectCalculator = 0 and fsl.Country = m.CountryId and fsl.Wg = m.WgId and fsl.ServiceLocation = m.ServiceLocationId and fsl.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceReactionTimeType fst on @isProjectCalculator = 0 and fst.Country = m.CountryId and fst.Wg = m.WgId and fst.ReactionTimeType = m.ReactionTime_ReactionType and fst.DeactivatedDateTime is null
		LEFT JOIN Hardware.FieldServiceAvailability fsa on @isProjectCalculator = 0 and fsa.Country = m.CountryId and fsa.Wg = m.WgId and fsa.[Availability] = m.AvailabilityId and fsa.DeactivatedDateTime is null

		LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.Deactivated = 0

		LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

		LEFT JOIN [References].ExchangeRate on @isProjectCalculator = 1 and Project.Reinsurance_CurrencyId = ExchangeRate.CurrencyId
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
            --, dur.Name             as Duration
            --, dur.Value            as Year
            --, dur.IsProlongation   as IsProlongation
			, m.Duration
			--, m.DurationValue as Year
			, m.DurationMonths / 12 as Year
			, m.DurationMonths
			, m.DurationIsProlongation as IsProlongation
			--, m.YearValue		   as StdYear
			, m.Months			 as StdMonths
			, m.IsProlongation   as StdIsProlongation
            , m.AvailabilityId
            --, av.Name              as Availability
			, m.Availability
            , m.ReactionTimeId
            --, rtime.Name           as ReactionTime
			, m.ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

            --, m.StdWarranty
			, m.StdWarrantyMonths / 12 as StdWarranty
			, m.StdWarrantyMonths
            , m.StdWarrantyLocation

            --Cost values

            --, std.AFR1  
            --, std.AFR2  
            --, std.AFR3  
            --, std.AFR4  
            --, std.AFR5  
            --, std.AFRP1 

			, m.AFR

            --, std.MatCost1
            --, std.MatCost2
            --, std.MatCost3
            --, std.MatCost4
            --, std.MatCost5
            --, std.MatCost1P

			, m.MatCost
			, m.MatW

            --, std.MatOow1 
            --, std.MatOow2 
            --, std.MatOow3 
            --, std.MatOow4 
            --, std.MatOow5 
            --, std.MatOow1p

			, m.MatOow

            --, std.MaterialW

            --, std.TaxAndDuties1
            --, std.TaxAndDuties2
            --, std.TaxAndDuties3
            --, std.TaxAndDuties4
            --, std.TaxAndDuties5
            --, std.TaxAndDuties1P

			, m.TaxW
			, m.TaxAndDuties

            --, std.TaxOow1 
            --, std.TaxOow2 
            --, std.TaxOow3 
            --, std.TaxOow4 
            --, std.TaxOow5 
            --, std.TaxOow1P

			, m.TaxOow
            
            --, std.TaxAndDutiesW

            --, ISNULL(case when @approved = 0 then r.Cost else r.Cost_approved end, 0) as Reinsurance
			, ISNULL(m.Cost, 0) as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
     --       , case when @approved = 0 
				 --  then 
					--	Hardware.CalcByFieldServicePerYear(
					--		fst.TimeAndMaterialShare_norm, 
					--		fsc.TravelCost, 
					--		fsc.LabourCost, 
					--		fst.PerformanceRate, 
					--		std.ExchangeRate,
					--		fsc.TravelTime,
					--		fsc.repairTime,
					--		std.OnsiteHourlyRates,
					--		UpliftFactor.OohUpliftFactor)
					--else
					--	Hardware.CalcByFieldServicePerYear(
					--		fst.TimeAndMaterialShare_norm_Approved, 
					--		fsc.TravelCost_Approved, 
					--		fsc.LabourCost_Approved, 
					--		fst.PerformanceRate_Approved, 
					--		std.ExchangeRate,
					--		fsc.TravelTime_Approved,
					--		fsc.repairTime_Approved,
					--		std.OnsiteHourlyRates,
					--		UpliftFactor.OohUpliftFactor_Approved)

     --          end as FieldServicePerYear

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
            --, case when dur.IsProlongation = 1 then std.ServiceSupportPerYearWithoutSar else std.ServiceSupportPerYear end as ServiceSupportPerYear

			, case when m.DurationIsProlongation = 1 then m.ServiceSupportPerYearWithoutSar else m.ServiceSupportPerYear end as ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
            --, case when @approved = 0 
            --       then lc.ExpressDelivery          +
            --            lc.HighAvailabilityHandling +
            --            lc.StandardDelivery         +
            --            lc.StandardHandling         +
            --            lc.ReturnDeliveryFactory    +
            --            lc.TaxiCourierDelivery      
            --       else lc.ExpressDelivery_Approved          +
            --            lc.HighAvailabilityHandling_Approved +
            --            lc.StandardDelivery_Approved         +
            --            lc.StandardHandling_Approved         +
            --            lc.ReturnDeliveryFactory_Approved    +
            --            lc.TaxiCourierDelivery_Approved     
            --    end / std.ExchangeRate as LogisticPerYear

			, (m.ExpressDelivery +
               m.HighAvailabilityHandling +
               m.StandardDelivery         +
               m.StandardHandling         +
               m.ReturnDeliveryFactory    +
               m.TaxiCourierDelivery) / m.ExchangeRate as LogisticPerYear

                                                                                                                       
            --, case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee
			, m.AvailabilityFee

            --, case when @approved = 0 
            --        then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
            --        else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
            --    end / std.ExchangeRate as MarkupOtherCost                      
            --, case when @approved = 0 
            --        then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
            --        else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
            --    end as MarkupFactorOtherCost                

			, case when m.DurationIsProlongation = 0 then m.Markup else m.ProlongationMarkup end / m.ExchangeRate as MarkupOtherCost     
			, case when m.DurationIsProlongation = 0 then m.MarkupFactor else m.ProlongationMarkupFactor end as MarkupFactorOtherCost     

            --####### PROACTIVE COST ###################
            --, case when proSla.Name = '0' 
            --        then 0 --we don't calc proactive(none)
            --        else std.LocalRemoteAccessSetup + dur.Value * (
            --                          std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
            --                        + std.LocalPreparation * proSla.LocalPreparationShcRepetition                      
            --                        + std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
            --                        + std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
            --                        + std.Travel * proSla.TravellingTimeRepetition                                     
            --                        + std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
            --                    )
            --    end as ProActive

			--, case when proSla.Name = '0' 
   --                 then 0 --we don't calc proactive(none)
   --                 else m.LocalRemoteAccessSetup + m.DurationValue * (
   --                                   m.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                
   --                                 + m.LocalPreparation * proSla.LocalPreparationShcRepetition                      
   --                                 + m.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition
   --                                 + m.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition
   --                                 + m.Travel * proSla.TravellingTimeRepetition                                     
   --                                 + m.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition          
   --                             )
   --             end as ProActive

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
            --, case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarranty       end as LocalServiceStandardWarranty
            --, case when dur.IsProlongation <> 1 then std.LocalServiceStandardWarrantyManual end as LocalServiceStandardWarrantyManual
			, case when m.DurationIsProlongation <> 1 then m.LocalServiceStandardWarranty       end as LocalServiceStandardWarranty
            , case when m.DurationIsProlongation <> 1 then m.LocalServiceStandardWarrantyManual end as LocalServiceStandardWarrantyManual
			, case when m.DurationIsProlongation <> 1 then Hardware.AddMarkup(m.LocalServiceStandardWarranty, m.RiskFactorStandardWarranty, m.RiskStandardWarranty) end as LocalServiceStandardWarrantyWithRisk

            --, std.Credit1 
            --, std.Credit2 
            --, std.Credit3 
            --, std.Credit4 
            --, std.Credit5 
            --, case when dur.IsProlongation <> 1 then std.Credits end as Credits

			--, case when dur.IsProlongation <> 1 then std.Credit end as Credit
			, case when m.DurationIsProlongation <> 1 then m.Credit end as Credit

            --########## MANUAL COSTS ################
            --, man.ListPrice          / std.ExchangeRate as ListPrice                   
            --, man.DealerDiscount                        as DealerDiscount              
            --, man.DealerPrice        / std.ExchangeRate as DealerPrice                 
            --, case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / std.ExchangeRate) end as ServiceTCManual                   
            --, case when std.CanOverrideTransferCostAndPrice = 1 then (man.ReActiveTP    / std.ExchangeRate) end as ReActiveTPManual                   
            --, man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  
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

    FROM ProjCalc m

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    LEFT JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.ManualCost man on @isProjectCalculator = 0 and man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId

    LEFT JOIN dbo.[User] u2 on u2.Id = man.ReleaseUserId

	WHERE m.Months <= m.DurationMonths and m.IsProlongation = m.DurationIsProlongation
)
go