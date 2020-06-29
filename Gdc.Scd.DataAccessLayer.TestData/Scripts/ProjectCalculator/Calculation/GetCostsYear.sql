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
	@projectId  BIGINT = NULL
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
       
        from Hardware.GetCalcMemberYear(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @projectId) m
    )
    , CostCte3 as (
        select    m.*
				, Hardware.MarkupOrFixValue(m.FieldServiceCost  + m.ServiceSupportPerYear + m.matCost  + m.Logistic  + m.Reinsurance + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect

		from CostCte m
    )
    , CostCte5 as (
        select m.*
			, m.FieldServiceCost + m.ServiceSupportPerYear + m.matCost + m.Logistic + m.TaxAndDuties + m.Reinsurance + m.OtherDirect + m.AvailabilityFeeOrZero - m.Credit as ServiceTP

        from CostCte3 m
    )
    , CostCte6 as (
        select m.*

				, m.ServiceTP - m.OtherDirect as ServiceTC

        from CostCte5 m
    )    

    select
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

			, m.AFR
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
			, m.[MarkupOtherCost]
			, m.MarkupFactorOtherCost
    from CostCte6 AS m
)
