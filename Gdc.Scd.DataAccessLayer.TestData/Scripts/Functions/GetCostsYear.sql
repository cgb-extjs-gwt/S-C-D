USE [SCD_2]

IF OBJECT_ID('[Hardware].[GetCostsYear]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCostsYear]
GO

CREATE FUNCTION [Hardware].[GetCostsYear](
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
    @limit int,
	@isProjectCalculator bit = 0
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , ISNULL(m.ProActive, 0) as ProActiveOrZero
                , ISNULL(m.AvailabilityFee, 0) as AvailabilityFeeOrZero
				
				, m.FieldServicePerYear * m.AFR  as FieldServiceCost
				, m.LogisticPerYear * m.AFR  as Logistic
       
        from Hardware.GetCalcMemberYear(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @isProjectCalculator) m
    )
    --, CostCte2 as (
    --    select    m.*

    --            --, m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
    --            --, m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
    --            --, m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
    --            --, m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
    --            --, m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
    --            --, m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

				--, m.FieldServicePerYear * m.AFR  as FieldServiceCost

    --            --, m.LogisticPerYear * m.AFR1  as Logistic1
    --            --, m.LogisticPerYear * m.AFR2  as Logistic2
    --            --, m.LogisticPerYear * m.AFR3  as Logistic3
    --            --, m.LogisticPerYear * m.AFR4  as Logistic4
    --            --, m.LogisticPerYear * m.AFR5  as Logistic5
    --            --, m.LogisticPerYear * m.AFRP1 as Logistic1P

				--, m.LogisticPerYear * m.AFR  as Logistic

    --            --, isnull(case when m.DurationId = 1 then m.Reinsurance end, 0) as Reinsurance1
    --            --, isnull(case when m.DurationId = 2 then m.Reinsurance end, 0) as Reinsurance2
    --            --, isnull(case when m.DurationId = 3 then m.Reinsurance end, 0) as Reinsurance3
    --            --, isnull(case when m.DurationId = 4 then m.Reinsurance end, 0) as Reinsurance4
    --            --, isnull(case when m.DurationId = 5 then m.Reinsurance end, 0) as Reinsurance5
    --            --, isnull(case when m.DurationId = 6 then m.Reinsurance end, 0) as Reinsurance1P

    --    from CostCte m
    --)
    , CostCte3 as (
        select    m.*

                --, Hardware.MarkupOrFixValue(m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.Reinsurance1 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                --, Hardware.MarkupOrFixValue(m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.Reinsurance2 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                --, Hardware.MarkupOrFixValue(m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.Reinsurance3 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                --, Hardware.MarkupOrFixValue(m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.Reinsurance4 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                --, Hardware.MarkupOrFixValue(m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.Reinsurance5 + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                --, Hardware.MarkupOrFixValue(m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.Reinsurance1P + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost) as OtherDirect1P

				, Hardware.MarkupOrFixValue(m.FieldServiceCost  + m.ServiceSupportPerYear + m.matCost  + m.Logistic  + m.Reinsurance + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect

        --from CostCte2 m
		from CostCte m
    )
    , CostCte5 as (
        select m.*

             --, m.FieldServiceCost1  + m.ServiceSupportPerYear + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.Reinsurance1 + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1 as ServiceTP1
             --, m.FieldServiceCost2  + m.ServiceSupportPerYear + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.Reinsurance2 + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2 as ServiceTP2
             --, m.FieldServiceCost3  + m.ServiceSupportPerYear + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.Reinsurance3 + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3 as ServiceTP3
             --, m.FieldServiceCost4  + m.ServiceSupportPerYear + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.Reinsurance4 + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4 as ServiceTP4
             --, m.FieldServiceCost5  + m.ServiceSupportPerYear + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.Reinsurance5 + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5 as ServiceTP5
             --, m.FieldServiceCost1P + m.ServiceSupportPerYear + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.Reinsurance1P + m.OtherDirect1P + m.AvailabilityFeeOrZero            as ServiceTP1P

			 --, Hardware.CalcByYear(
				--m.Year, 
				--m.StdYear, 
				--m.IsProlongation, 
				--m.StdIsProlongation, 
				--m.FieldServiceCost + m.ServiceSupportPerYear + m.matCost + m.Logistic + m.TaxAndDuties + m.Reinsurance + m.OtherDirect + m.AvailabilityFeeOrZero - m.Credit) as ServiceTP

			, m.FieldServiceCost + m.ServiceSupportPerYear + m.matCost + m.Logistic + m.TaxAndDuties + m.Reinsurance + m.OtherDirect + m.AvailabilityFeeOrZero - m.Credit as ServiceTP

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

                --, m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
                --, m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
                --, m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
                --, m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
                --, m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
                --, m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P

				--, case when m.StdIsProlongation = 1 then 0 else m.LocalServiceStandardWarranty end as LocalServiceStandardWarrantyForSum
				--, case when m.StdIsProlongation = 1 then 0 else m.Credit end as CreditForSum

				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.ServiceTP - m.OtherDirect) as ServiceTC
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.TaxOow) as TaxOowForSum
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.MatOow) as MaterialOowForSum
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.FieldServiceCost) as FieldServiceCostForSum
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.Logistic) as LogisticForSum
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.OtherDirect) as OtherDirectForSum
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.TaxW) as TaxWForSum
				--, Hardware.CalcByYear(m.Year, m.StdYear, m.IsProlongation, m.StdIsProlongation, m.MatW) as MatWForSum

				, m.ServiceTP - m.OtherDirect as ServiceTC

        from CostCte5 m
    )    

    select --m.*

            --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ReActiveTC
            --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ReActiveTP 
			 
			m.rownum
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
			, m.DurationMonths
			, m.StdIsProlongation
		    , m.StdMonths
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

			--Costs
			, m.AvailabilityFee
			, m.Reinsurance
			, m.ProActive
			, m.ServiceSupportPerYear
			, m.LocalServiceStandardWarrantyManual
			, m.ProActiveOrZero
			, m.ListPrice
		    , m.DealerDiscount
			, m.DealerPrice
			, m.ServiceTCManual
			, m.ReActiveTPManual
			, m.ServiceTP_Released

			, m.ServiceTP
			, m.LocalServiceStandardWarranty
			, coalesce(m.LocalServiceStandardWarrantyManual, m.LocalServiceStandardWarrantyWithRisk, m.LocalServiceStandardWarranty) as LocalServiceStandardWarrantyWithRisk
			, m.Credit
			, m.ServiceTC
			, m.TaxOow
			, m.MatOow
			, m.FieldServiceCost
			, m.Logistic
			, m.OtherDirect
			, m.TaxW
			, m.MatW

			, m.ReleaseDate
			, m.ReleaseUserName
			, m.ReleaseUserEmail

			, m.ChangeDate
			, m.ChangeUserName
			, m.ChangeUserEmail
    from CostCte6 AS m
)
