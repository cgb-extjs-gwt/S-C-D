USE [SCD_2]

IF OBJECT_ID('[Hardware].[GetCostsNew]') IS NOT NULL
    DROP FUNCTION [Hardware].[GetCostsNew]
GO

CREATE FUNCTION [Hardware].[GetCostsNew](
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
     WITH CostCte as (
        select --m.*

             --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ReActiveTC
             --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ReActiveTP 
			 
			   m.rownum
			 , m.PortfolioId as Id

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
			 , m.ProActiveOrZero
			 , m.ListPrice
		     , m.DealerDiscount
			 , m.DealerPrice
			 , m.ServiceTCManual
			 , m.ReActiveTPManual
			 , m.ServiceTP_Released

			 --, SUM(m.ServiceTC) as ReActiveTC
			 , SUM(m.ServiceTP) as ReActiveTP
			 --, SUM(m.TaxOowForSum) as TaxAndDutiesOow
			 --, SUM(m.MaterialOowForSum) as MaterialOow
			 --, SUM(m.FieldServiceCostForSum) as FieldServiceCost
			 --, SUM(m.LogisticForSum) as Logistic
			 --, SUM(m.OtherDirectForSum) as OtherDirect
			 --, SUM(m.LocalServiceStandardWarrantyForSum) as LocalServiceStandardWarranty
			 --, SUM(m.CreditForSum) as Credits
			 --, SUM(m.TaxWForSum) as TaxAndDutiesW
			 --, SUM(m.MatWForSum) as MaterialW

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
        from [Hardware].[GetCostsYear](@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit, @isProjectCalculator) m
		group by 
			   m.rownum
			 , m.PortfolioId

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
         --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.TaxOow1, m.TaxOow2, m.TaxOow3, m.TaxOow4, m.TaxOow5, m.TaxOow1P) as TaxAndDutiesOow
		 , m.TaxAndDutiesOow

         , m.Reinsurance
         , m.ProActive
         , m.Year * m.ServiceSupportPerYear as ServiceSupportCost

         , m.MaterialW
         --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.MatOow1, m.MatOow2, m.MatOow3, m.MatOow4, m.MatOow5, m.MatOow1P) as MaterialOow
		 , m.MaterialOow

         --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
		 , m.FieldServiceCost
         --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
		 , m.Logistic
         --, Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
		 , m.OtherDirect
       
         , m.LocalServiceStandardWarranty
         , m.LocalServiceStandardWarrantyManual
       
         , m.Credits

         , m.ReActiveTC
         , m.ReActiveTC + m.ProActiveOrZero ServiceTC
         
         , m.ReActiveTP
         , m.ReActiveTP + m.ProActiveOrZero as ServiceTP

         --, m.ServiceTC1
         --, m.ServiceTC2
         --, m.ServiceTC3
         --, m.ServiceTC4
         --, m.ServiceTC5
         --, m.ServiceTC1P

         --, m.ServiceTP1
         --, m.ServiceTP2
         --, m.ServiceTP3
         --, m.ServiceTP4
         --, m.ServiceTP5
         --, m.ServiceTP1P

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

