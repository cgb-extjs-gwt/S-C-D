alter table Hardware.ManualCost add ServiceTP_Released_Temp float;
go

update Hardware.ManualCost set ServiceTP_Released_Temp = ServiceTP_Released;
go

alter table Hardware.ManualCost drop column ServiceTP_Released;
go

EXEC sp_rename 'Hardware.ManualCost.ServiceTP_Released_Temp', 'ServiceTP_Released', 'COLUMN'
go

ALTER FUNCTION [Hardware].[GetReleaseCosts] (
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
    with ReinsuranceCte as (
        select r.Wg
             , ta.ReactionTimeId
             , ta.AvailabilityId
             , r.ReinsuranceFlatfee_norm_Approved / er.Value as Cost
        from Hardware.Reinsurance r

        JOIN Dependencies.ReactionTime_Avalability ta on ta.Id = r.ReactionTimeAvailability and ta.IsDisabled = 0

        JOIN [References].ExchangeRate er on er.CurrencyId = r.CurrencyReinsurance_Approved

        where r.Duration = (select id from Dependencies.Duration where IsProlongation = 1 and Value = 1)
              and r.DeactivatedDateTime is null
    )
    , CostCte as (
        select    m.*

                , coalesce(m.AvailabilityFee, 0) as AvailabilityFeeOrZero

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive

                , coalesce(r.Cost, 0) as ReinsuranceProlCost
       
        from Hardware.GetCalcMember(1, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
        left join ReinsuranceCte r on r.Wg = m.WgId and r.ReactionTimeId = m.ReactionTimeId and r.AvailabilityId = m.AvailabilityId
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

                , coalesce(case when m.DurationId = 1 then m.Reinsurance end, 0) as Reinsurance1
                , coalesce(case when m.DurationId = 2 then m.Reinsurance end, 0) as Reinsurance2
                , coalesce(case when m.DurationId = 3 then m.Reinsurance end, 0) as Reinsurance3
                , coalesce(case when m.DurationId = 4 then m.Reinsurance end, 0) as Reinsurance4
                , coalesce(case when m.DurationId = 5 then m.Reinsurance end, 0) as Reinsurance5
                , coalesce(case when m.DurationId = 6 then m.Reinsurance end, 0) as Reinsurance1P

        from CostCte m
    )
    , CostCte3 as (
        select    m.*

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.ReinsuranceProlCost + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1  + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect_final_1
                , Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2  + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect_final_2
                , Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3  + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect_final_3
                , Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4  + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect_final_4
                , Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5  + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect_final_5
                , Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect_final_1P

        from CostCte2 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceProlCost + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceProlCost + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceProlCost + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceProlCost + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceProlCost + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceProlCost + m.OtherDirect1P + m.AvailabilityFeeOrZero             as ServiceTP1P


             , m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1  + m.OtherDirect_final_1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP_final_1
             , m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2  + m.OtherDirect_final_2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP_final_2
             , m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3  + m.OtherDirect_final_3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP_final_3
             , m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4  + m.OtherDirect_final_4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP_final_4
             , m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5  + m.OtherDirect_final_5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP_final_5
             , m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect_final_1P + m.AvailabilityFeeOrZero             as ServiceTP_final_1P

        from CostCte3 m
    )
    select m.Id

         --SLA

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

         --Cost

         , Hardware.PositiveValue(Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP_final_1, m.ServiceTP_final_2, m.ServiceTP_final_3, m.ServiceTP_final_4, m.ServiceTP_final_5, m.ServiceTP_final_1P)) as ServiceTP

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
         , m.ServiceTP_Released

         , m.ReleaseDate
         , m.ChangeUserName
         , m.ChangeUserEmail

       from CostCte5 m
)
GO

ALTER PROCEDURE [Hardware].[SpReleaseCosts]
    @usr          int, 
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @portfolioIds dbo.ListID readonly
AS
BEGIN

    SET NOCOUNT ON;
    

	SELECT * INTO #temp 
	FROM Hardware.GetReleaseCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, 0, 0) costs
	WHERE (not exists(select 1 from @portfolioIds) or costs.Id in (select Id from @portfolioIds))   
	--TODO: @portfolioIds case to be fixed in a future release 

	UPDATE mc
	SET [ServiceTP1_Released]  = case when dur.Value >= 1 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP1) end,
		[ServiceTP2_Released]  = case when dur.Value >= 2 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP2) end,
		[ServiceTP3_Released]  = case when dur.Value >= 3 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP3) end,
		[ServiceTP4_Released]  = case when dur.Value >= 4 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP4) end,
		[ServiceTP5_Released]  = case when dur.Value >= 5 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP5) end,
		[ServiceTP1P_Released] = case when dur.IsProlongation = 1                    then  COALESCE(costs.ServiceTPManual, costs.ServiceTP1P)            end,

		[ServiceTP_Released]   = COALESCE(costs.ServiceTPManual, costs.ServiceTP),

		[ChangeUserId] = @usr,
        [ReleaseDate] = getdate()
	FROM [Hardware].[ManualCost] mc
	JOIN #temp costs on mc.PortfolioId = costs.Id
	JOIN Dependencies.Duration dur on costs.DurationId = dur.Id
	where costs.ServiceTPManual is not null or costs.ServiceTP is not null

	INSERT INTO [Hardware].[ManualCost] 
				([PortfolioId], 
				[ChangeUserId], 
                [ReleaseDate],
				[ServiceTP1_Released], [ServiceTP2_Released], [ServiceTP3_Released], [ServiceTP4_Released], [ServiceTP5_Released], [ServiceTP1P_Released], ServiceTP_Released)
	SELECT  costs.Id, 
			@usr, 
            getdate(),
			case when dur.Value >= 1 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP1) end,
			case when dur.Value >= 2 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP2) end,
			case when dur.Value >= 3 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP3) end,
			case when dur.Value >= 4 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP4) end,
			case when dur.Value >= 5 and dur.IsProlongation = 0 then  COALESCE(costs.ServiceTPManual / dur.Value, costs.ServiceTP5) end,
			case when dur.IsProlongation = 1                    then  COALESCE(costs.ServiceTPManual, costs.ServiceTP1P)            end,
            COALESCE(costs.ServiceTPManual, costs.ServiceTP)
	FROM [Hardware].[ManualCost] mc
	RIGHT JOIN #temp costs on mc.PortfolioId = costs.Id
	JOIN Dependencies.Duration dur on costs.DurationId = dur.Id
	where mc.PortfolioId is null and (costs.ServiceTPManual is not null or costs.ServiceTP is not null)

	DROP table #temp
   
END
GO

