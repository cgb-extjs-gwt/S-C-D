IF OBJECT_ID('Report.LogisticCostCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCountry;
go 

CREATE FUNCTION Report.LogisticCostCountry
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select Region
         , Country
         , Wg

         , Currency

         , ReactionType

         , StandardHandling
         , HighAvailabilityHandling
         , StandardDelivery
         , ExpressDelivery
         , TaxiCourierDelivery
         , ReturnDeliveryFactory

    from Report.LogisticCostCentral(@cnt, @wg, @reactiontime, @reactiontype)
)

GO

IF OBJECT_ID('Report.LogisticCostCalcCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCalcCountry;
go 

CREATE FUNCTION Report.LogisticCostCalcCountry
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    select rep.Id
         , rep.Region
         , rep.Country
         , rep.Wg

         , rep.ServiceLevel
         , rep.ReactionTime
         , rep.ReactionType
         , rep.Duration
         , rep.Availability
         , rep.ProActiveSla

         , rep.ServiceTC * er.Value as ServiceTC
         , rep.Handling * er.Value as Handling
         , rep.TaxAndDutiesW * er.Value as TaxAndDutiesW
         , rep.TaxAndDutiesOow * er.Value as TaxAndDutiesOow

         , rep.LogisticW * er.Value as LogisticW
         , rep.LogisticOow * er.Value as LogisticOow

         , rep.Fee * er.Value as Fee
		 , cur.Name as Currency
    from Report.LogisticCostCalcCentral(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) rep
	join InputAtoms.Country c on c.Id = @cnt
	join [References].Currency cur on cur.Id = c.CurrencyId
	join [References].ExchangeRate er on er.CurrencyId = cur.Id
)

GO

IF OBJECT_ID('Report.LogisticCostInputCountry') IS NOT NULL
  DROP FUNCTION Report.LogisticCostInputCountry;
go 

CREATE FUNCTION Report.LogisticCostInputCountry
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select Region
         , Country
         , Wg

         , Currency

         , ReactionType

         , StandardHandling
         , HighAvailabilityHandling
         , StandardDelivery
         , ExpressDelivery
         , TaxiCourierDelivery
         , ReturnDeliveryFactory

    FROM Report.LogisticCostInputCentral(@cnt, @wg, @reactiontime, @reactiontype)
)

GO