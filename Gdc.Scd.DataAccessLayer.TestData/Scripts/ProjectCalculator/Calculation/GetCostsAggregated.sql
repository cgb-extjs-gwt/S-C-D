USE [SCD_2]

IF OBJECT_ID('[Hardware].[GetCostsAggregated]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCostsAggregated]
GO

CREATE FUNCTION [Hardware].[GetCostsAggregated](
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
     WITH CostCte as (
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
			 , m.LocalServiceStandardWarrantyWithRisk
			 , m.ProActiveOrZero
			 , m.ListPrice
		     , m.DealerDiscount
			 , m.DealerPrice
			 , m.ServiceTCManual
			 , m.ReActiveTPManual
			 , m.ServiceTP_Released

			 , SUM(m.ServiceTP) as ReActiveTP
			 , SUM(LocalServiceStandardWarranty) as LocalServiceStandardWarranty
			 , SUM(m.Credit) as Credits
			 , SUM(m.ServiceTC) as ReActiveTC
			 , SUM(m.TaxOow) as TaxAndDutiesOow
			 , SUM(m.MatOow) as MaterialOow
			 , SUM(m.FieldServiceCost) as FieldServiceCost
			 , SUM(m.Logistic) as Logistic
			 , SUM(m.OtherDirect) as OtherDirect
			 , SUM(m.TaxW) as TaxAndDutiesW
			 , SUM(m.MatW) as MaterialW

			 , m.ReleaseDate
			 , m.ReleaseUserName
			 , m.ReleaseUserEmail

			 , m.ChangeDate
			 , m.ChangeUserName
			 , m.ChangeUserEmail
        from [Hardware].[GetCostsYear](@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @projectId) m
		group by 
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
			 , m.LocalServiceStandardWarrantyWithRisk
			 , m.ProActiveOrZero
			 , m.ListPrice
		     , m.DealerDiscount
			 , m.DealerPrice
			 , m.ServiceTCManual
			 , m.ReActiveTPManual

			 , m.ServiceTP_Released

			 , m.ReleaseDate
			 , m.ReleaseUserName
			 , m.ReleaseUserEmail

			 , m.ChangeDate
			 , m.ChangeUserName
			 , m.ChangeUserEmail
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
		 , m.TaxAndDutiesOow

         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.MaterialW
		 , m.MaterialOow

		 , m.FieldServiceCost
		 , m.Logistic
		 , m.OtherDirect
       
         , m.LocalServiceStandardWarranty
         , m.LocalServiceStandardWarrantyManual
		 , m.LocalServiceStandardWarrantyWithRisk
       
         , m.Credits

         , m.ReActiveTC
         , m.ReActiveTC + m.ProActiveOrZero ServiceTC
         
         , m.ReActiveTP
         , m.ReActiveTP + m.ProActiveOrZero as ServiceTP

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ReActiveTPManual 
         , m.ReActiveTPManual + m.ProActiveOrZero as ServiceTPManual
         
         , coalesce(m.ServiceTCManual, m.ReactiveTC + m.ProActiveOrZero) as ServiceTCResult
         , coalesce(m.ReActiveTPManual, m.ReActiveTP) as ReActiveTPResult
         , coalesce(m.ReActiveTPManual, m.ReActiveTP) + m.ProActiveOrZero as ServiceTPResult
         , m.ServiceTP_Released

         , m.ReleaseDate
         , m.ReleaseUserName
         , m.ReleaseUserEmail

         , m.ChangeDate
         , m.ChangeUserName
         , m.ChangeUserEmail

    from CostCte m
)
GO

